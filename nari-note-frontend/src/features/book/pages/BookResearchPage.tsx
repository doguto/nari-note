'use client';

import { useEffect, useMemo, useState } from 'react';
import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter, useSearchParams } from 'next/navigation';
import { ErrorMessage, LoadingSpinner } from '@/components/ui';
import {
  booksApi,
  queryKeys,
  useDeleteBookMove,
  useGetBook,
  useGetBookPosition,
  useMergeShapeCandidate,
  useRejectShapeCandidate,
  useSaveBookLines,
  useSplitShapeGroup,
  useUpdateBookMove,
  useUpdateShapeGroup,
} from '@/lib/api';
import { useAnalysisEngine } from '../hooks/useAnalysisEngine';
import { useResearchDraft } from '../hooks/useResearchDraft';
import { type ResearchMove, type ResearchView, BookResearchTemplate } from '../templates';
import { destinationOf, legalMoves, moveLabel, play as playOn } from '../utils/shogiRules';

interface BookResearchPageProps {
  bookId: number;
}

const JUDGE_POLL_INTERVAL_MS = 10_000;

function isTyping(target: EventTarget | null): boolean {
  return target instanceof HTMLElement && (target.isContentEditable || ['INPUT', 'TEXTAREA', 'SELECT'].includes(target.tagName));
}

export function BookResearchPage({ bookId }: BookResearchPageProps) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const queryClient = useQueryClient();
  const [errorMessage, setErrorMessage] = useState<string>();
  const [selected, setSelected] = useState<{ key: string; index: number } | null>(null);
  // 保存済みの局面へ進むときは、次の局面が届くまで手元で進めた盤を見せる
  const [pending, setPending] = useState<{ fromPositionId: number; sfen: string; usi: string } | null>(null);
  // 保存していない局面を見ているときの、その局面の SFEN
  const [draftSfen, setDraftSfen] = useState<string | null>(null);
  const draft = useResearchDraft();
  const [engine, setEngine] = useAnalysisEngine();

  const book = useGetBook({ id: bookId });
  const requested = Number(searchParams.get('p'));
  const positionId = requested > 0 ? requested : book.data?.rootPositionId;
  const enabled = positionId !== undefined;

  const position = useGetBookPosition(
    { id: bookId, positionId: positionId ?? 0 },
    {
      enabled,
      placeholderData: keepPreviousData,
      // 裏で同じ形の判定が進んでいる間は、候補が出たら見えるように取り直す
      refetchInterval: (query) => ((query.state.data?.pendingJudgeCount ?? 0) > 0 ? JUDGE_POLL_INTERVAL_MS : false),
    },
  );
  const data = position.data;

  if (pending && data && !position.isPlaceholderData && data.positionId !== pending.fromPositionId) setPending(null);
  const optimistic = pending && (position.isPlaceholderData || data?.positionId === pending.fromPositionId) ? pending : null;

  const draftLine = draftSfen ? draft.lineTo(draftSfen) : null;
  const viewSfen = optimistic?.sfen ?? draftSfen ?? data?.sfen;

  const analysis = useQuery({
    queryKey: ['bookAnalysis', bookId, viewSfen, engine],
    queryFn: () => booksApi.analyzeBookPosition({ id: bookId, sfen: viewSfen!, engine }),
    enabled: !!viewSfen && !optimistic,
    staleTime: Infinity,
    retry: false,
  });

  const navigate = (id: number) => {
    setErrorMessage(undefined);
    setDraftSfen(null);
    router.push(`/books/${bookId}?p=${id}`);
  };
  const onError = (message: string) => () => setErrorMessage(message);

  const save = useSaveBookLines({
    onSuccess: (res) => {
      const saved = draftSfen ? res.positions.find((p) => p.sfen === draftSfen) : undefined;
      draft.clear();
      if (saved) navigate(saved.positionId);
    },
    onError: onError('保存できませんでした。'),
  });
  const updateMove = useUpdateBookMove({ onError: onError('保存できませんでした。') });
  const deleteMove = useDeleteBookMove({ onError: onError('手を外せませんでした。') });
  const updateGroup = useUpdateShapeGroup({ onError: onError('保存できませんでした。') });
  const merge = useMergeShapeCandidate({ onError: onError('まとめられませんでした。') });
  const reject = useRejectShapeCandidate({ onError: onError('保存できませんでした。') });
  const split = useSplitShapeGroup({ onError: onError('分けられませんでした。') });

  useEffect(() => {
    if (!data || position.isPlaceholderData) return;
    // 次に開きそうな局面（進む手の先と 1 手前）を先に取っておき、手送りで待たせない
    const neighbors = [...data.moves.map((m) => m.childPositionId), ...data.path.slice(-1).map((s) => s.parentPositionId)];
    for (const id of neighbors) {
      queryClient.prefetchQuery({
        queryKey: [...queryKeys.books.getBookPosition, { id: bookId, positionId: id }],
        queryFn: () => booksApi.getBookPosition({ id: bookId, positionId: id }),
        staleTime: 30_000,
      });
    }
  }, [data, position.isPlaceholderData, bookId, queryClient]);

  useEffect(() => {
    if (draft.count === 0) return;
    const warn = (e: BeforeUnloadEvent) => e.preventDefault();
    window.addEventListener('beforeunload', warn);
    return () => window.removeEventListener('beforeunload', warn);
  }, [draft.count]);

  const view = useMemo((): ResearchView | null => {
    if (!data) return null;
    if (optimistic) {
      return { sfen: optimistic.sfen, legalMoves: [], lastMoveUsi: optimistic.usi, ply: data.path.length + 1, moves: [], isDraft: false };
    }
    if (draftSfen && draftLine) {
      const node = draftLine.nodes[draftLine.nodes.length - 1];
      const moves: ResearchMove[] = draft.childrenOf({ kind: 'draft', sfen: draftSfen }).map((n, i) => ({
        id: -(i + 1),
        usi: n.usi,
        label: n.label,
        memo: '',
        condition: '',
        conditionSuggestions: [],
        childPositionId: 0,
        isDraft: true,
      }));
      return {
        sfen: draftSfen,
        legalMoves: legalMoves(draftSfen),
        lastMoveUsi: node.usi,
        lastMoveLabel: node.label,
        ply: data.path.length + draftLine.nodes.length,
        moves,
        isDraft: true,
      };
    }
    const savedUsis = new Set(data.moves.map((m) => m.usi));
    const drafts: ResearchMove[] = draft
      .childrenOf({ kind: 'saved', positionId: data.positionId })
      .filter((n) => !savedUsis.has(n.usi))
      .map((n, i) => ({ id: -(i + 1), usi: n.usi, label: n.label, memo: '', condition: '', conditionSuggestions: [], childPositionId: 0, isDraft: true }));
    return {
      sfen: data.sfen,
      legalMoves: data.legalMoves,
      lastMoveUsi: data.lastMoveUsi,
      lastMoveLabel: data.path[data.path.length - 1]?.label,
      ply: data.path.length,
      moves: [...data.moves, ...drafts],
      isDraft: false,
    };
  }, [data, optimistic, draftSfen, draftLine, draft]);

  const viewKey = draftSfen ?? String(data?.positionId ?? '');
  const selectedIndex = selected && selected.key === viewKey ? selected.index : -1;

  const play = (usi: string) => {
    if (!data || !view) return;
    setErrorMessage(undefined);
    if (!draftSfen) {
      const saved = data.moves.find((m) => m.usi === usi);
      if (saved) {
        setPending({ fromPositionId: data.positionId, sfen: playOn(data.sfen, usi), usi });
        return navigate(saved.childPositionId);
      }
    }
    const previousTo = view.lastMoveUsi ? destinationOf(view.lastMoveUsi) : undefined;
    const childSfen = playOn(view.sfen, usi);
    draft.add({
      sfen: childSfen,
      parent: draftSfen ? { kind: 'draft', sfen: draftSfen } : { kind: 'saved', positionId: data.positionId },
      usi,
      label: moveLabel(view.sfen, usi, previousTo),
    });
    setDraftSfen(childSfen);
  };

  const rootPositionId = book.data?.rootPositionId;
  const goFirst = () => rootPositionId !== undefined && navigate(rootPositionId);
  const goBack = () => {
    if (draftLine) {
      const node = draftLine.nodes[draftLine.nodes.length - 1];
      setDraftSfen(node.parent.kind === 'draft' ? node.parent.sfen : null);
      return;
    }
    if (data && data.path.length > 0) navigate(data.path[data.path.length - 1].parentPositionId);
  };
  const goForward = () => {
    const move = view?.moves[selectedIndex >= 0 ? selectedIndex : 0];
    if (move) play(move.usi);
  };

  useEffect(() => {
    if (!view) return;
    const handler = (e: KeyboardEvent) => {
      if (isTyping(e.target) || e.altKey || e.ctrlKey || e.metaKey) return;
      if (e.key === 'ArrowLeft') {
        e.preventDefault();
        goBack();
      } else if (e.key === 'ArrowRight') {
        e.preventDefault();
        goForward();
      } else if ((e.key === 'ArrowDown' || e.key === 'ArrowUp') && view.moves.length > 0) {
        e.preventDefault();
        const count = view.moves.length;
        const step = e.key === 'ArrowDown' ? 1 : -1;
        const next = selectedIndex < 0 ? (step > 0 ? 0 : count - 1) : (selectedIndex + step + count) % count;
        setSelected({ key: viewKey, index: next });
      }
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  });

  if (book.isLoading || (enabled && position.isLoading && !data)) {
    return <LoadingSpinner text="定跡を読み込み中..." />;
  }
  if (book.error || position.error || !data || !view) {
    return <ErrorMessage message="定跡の取得に失敗しました" onRetry={() => position.refetch()} />;
  }

  const isBusy = !!optimistic || save.isPending || deleteMove.isPending || merge.isPending || reject.isPending || split.isPending;

  return (
    <BookResearchTemplate
      data={data}
      view={view}
      draft={{
        count: draft.count,
        isSaving: save.isPending,
        onSave: () => save.mutate({ id: bookId, lines: draft.lines }),
        onDiscard: () => {
          draft.clear();
          setDraftSfen(null);
        },
      }}
      analysis={{
        isLoading: analysis.isLoading,
        isAvailable: analysis.data?.isAvailable ?? false,
        hasError: !!analysis.error,
        lines: analysis.data?.lines ?? [],
        engine,
        onEngineChange: setEngine,
      }}
      isBusy={isBusy}
      errorMessage={errorMessage}
      selectedMoveIndex={selectedIndex}
      onSelectMove={(index) => setSelected({ key: viewKey, index })}
      onNavigate={navigate}
      onFirst={goFirst}
      onBack={goBack}
      onForward={goForward}
      onMove={play}
      onMoveChange={(moveId, change) => updateMove.mutate({ id: bookId, moveId, ...change })}
      onDeleteMove={(moveId) => deleteMove.mutate({ id: bookId, moveId })}
      onRenameGroup={(name) => updateGroup.mutate({ id: bookId, groupId: data.group.id, name })}
      onSaveMemo={(memo) => updateGroup.mutate({ id: bookId, groupId: data.group.id, memo })}
      onMerge={(candidateId) => merge.mutate({ id: bookId, candidateId })}
      onReject={(candidateId) => reject.mutate({ id: bookId, candidateId })}
      onSplit={(id) => split.mutate({ id: bookId, positionId: id })}
    />
  );
}
