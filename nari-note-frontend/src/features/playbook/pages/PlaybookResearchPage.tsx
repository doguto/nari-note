'use client';

import { useEffect, useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useRouter, useSearchParams } from 'next/navigation';
import { ErrorMessage, LoadingSpinner } from '@/components/ui';
import {
  playbooksApi,
  useGetPlaybook,
  useInspectPlaybookBoard,
  useMergePlaybookNodes,
  useRemovePlaybookMove,
  useSavePlaybookLines,
  useSplitPlaybookBoard,
  useUpdatePlaybookNode,
  useUpdatePlaybookStep,
} from '@/lib/api';
import { useAnalysisEngine } from '../hooks/useAnalysisEngine';
import { useResearchDraft } from '../hooks/useResearchDraft';
import type { ResearchMove } from '../organisms';
import { type ResearchShape, type ResearchView, PlaybookResearchTemplate } from '../templates';
import { buildPlaybookTree, contains, pathTo } from '../utils/playbookTree';
import { destinationOf, legalMoves, moveLabel, play as playOn } from '../utils/shogiRules';

interface PlaybookResearchPageProps {
  playbookId: string;
}

function isTyping(target: EventTarget | null): boolean {
  return target instanceof HTMLElement && (target.isContentEditable || ['INPUT', 'TEXTAREA', 'SELECT'].includes(target.tagName));
}

// 定跡の全体を 1 回取得し、手順を手元で指し直して局面を行き来する。URL にはノードの盤面だけを載せる
export function PlaybookResearchPage({ playbookId }: PlaybookResearchPageProps) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [errorMessage, setErrorMessage] = useState<string>();
  const [selected, setSelected] = useState<{ sfen: string; index: number } | null>(null);
  const [current, setCurrent] = useState<string | null>(null);
  const draft = useResearchDraft();
  const [engine, setEngine] = useAnalysisEngine();

  const playbook = useGetPlaybook({ id: playbookId });
  const tree = useMemo(() => (playbook.data ? buildPlaybookTree(playbook.data) : null), [playbook.data]);

  const requestedBoard = searchParams.get('b');
  const sfen = useMemo(() => {
    if (!tree) return null;
    if (current && (contains(tree, current) || draft.get(current))) return current;
    return (requestedBoard && tree.boardsById.get(requestedBoard)?.sfen) || tree.rootSfen;
  }, [tree, current, requestedBoard, draft]);

  const isSaved = !!tree && !!sfen && contains(tree, sfen);
  const inspection = useInspectPlaybookBoard({ id: playbookId, sfen: sfen ?? '' }, { enabled: isSaved, staleTime: 30_000 });
  const analysis = useQuery({
    queryKey: ['playbookAnalysis', playbookId, sfen, engine],
    queryFn: () => playbooksApi.analyzePlaybookBoard({ id: playbookId, sfen: sfen!, engine }),
    enabled: !!sfen,
    staleTime: Infinity,
    retry: false,
  });

  const navigate = (next: string) => {
    setErrorMessage(undefined);
    setCurrent(next);
    const board = tree?.boards.get(next);
    if (board) router.replace(`/playbooks/${playbookId}?b=${board.id}`, { scroll: false });
  };
  const onError = (message: string) => () => setErrorMessage(message);

  const save = useSavePlaybookLines({
    // 保存した局面が定跡の取得し直しで現れるまで、下書きを残して盤を保つ
    onSuccess: async () => {
      await playbook.refetch();
      draft.clear();
    },
    onError: onError('保存できませんでした。'),
  });
  const removeMove = useRemovePlaybookMove({ onError: onError('手を外せませんでした。') });
  const updateNode = useUpdatePlaybookNode({ onError: onError('保存できませんでした。') });
  const updateStep = useUpdatePlaybookStep({ onError: onError('保存できませんでした。') });
  const merge = useMergePlaybookNodes({ onError: onError('まとめられませんでした。') });
  const split = useSplitPlaybookBoard({ onError: onError('分けられませんでした。') });

  useEffect(() => {
    if (draft.count === 0) return;
    const warn = (e: BeforeUnloadEvent) => e.preventDefault();
    window.addEventListener('beforeunload', warn);
    return () => window.removeEventListener('beforeunload', warn);
  }, [draft.count]);

  const path = useMemo(() => {
    if (!tree || !sfen) return [];
    if (contains(tree, sfen)) return pathTo(tree, sfen);
    const line = draft.lineTo(sfen);
    if (!line) return [];
    return [...pathTo(tree, line.anchorSfen), ...line.nodes.map((n) => ({ sfen: n.parentSfen, usi: n.usi, label: n.label }))];
  }, [tree, sfen, draft]);

  const view = useMemo((): ResearchView | null => {
    if (!tree || !sfen) return null;
    const last = path[path.length - 1];
    const previousTo = last ? destinationOf(last.usi) : undefined;
    const suggestions = new Map((inspection.data?.conditionSuggestions ?? []).map((c) => [c.stepId, c.suggestions]));
    const saved: ResearchMove[] = (tree.children.get(sfen) ?? []).map((m) => ({
      usi: m.usi,
      label: moveLabel(sfen, m.usi, previousTo),
      isDraft: false,
      stepId: m.step?.id,
      condition: m.step?.condition ?? '',
      conditionSuggestions: (m.step && suggestions.get(m.step.id)) ?? [],
    }));
    const savedUsis = new Set(saved.map((m) => m.usi));
    const drafts: ResearchMove[] = draft
      .childrenOf(sfen)
      .filter((n) => !savedUsis.has(n.usi))
      .map((n) => ({ usi: n.usi, label: n.label, isDraft: true, condition: '', conditionSuggestions: [] }));
    return {
      sfen,
      legalMoves: legalMoves(sfen),
      lastMoveUsi: last?.usi,
      lastMoveLabel: last?.label,
      ply: path.length,
      moves: [...saved, ...drafts],
      isDraft: !contains(tree, sfen),
    };
  }, [tree, sfen, path, draft, inspection.data]);

  const shape = useMemo((): ResearchShape => {
    const board = sfen ? tree?.boards.get(sfen) : undefined;
    const node = board ? tree?.nodes.get(board.nodeId) : undefined;
    const boardCount = board && tree ? [...tree.boards.values()].filter((b) => b.nodeId === board.nodeId).length : 0;
    const info = inspection.data?.boardId === board?.id ? inspection.data : undefined;
    return {
      isNode: !!board,
      name: node?.name ?? '',
      memo: node?.memo ?? '',
      boardCount,
      nameSuggestions: inspection.data?.nameSuggestions ?? [],
      members: info?.members ?? [],
      similar: info?.similar ?? [],
      boardId: board?.id,
    };
  }, [tree, sfen, inspection.data]);

  const selectedIndex = selected && selected.sfen === sfen ? selected.index : -1;

  const play = (usi: string) => {
    if (!tree || !sfen || !view) return;
    setErrorMessage(undefined);
    const child = playOn(sfen, usi);
    if (!contains(tree, child)) {
      const previousTo = view.lastMoveUsi ? destinationOf(view.lastMoveUsi) : undefined;
      draft.add({ sfen: child, parentSfen: sfen, usi, label: moveLabel(sfen, usi, previousTo) });
    }
    navigate(child);
  };

  const goFirst = () => tree && navigate(tree.rootSfen);
  const goBack = () => {
    const previous = path[path.length - 1];
    if (previous) navigate(previous.sfen);
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
        setSelected({ sfen: view.sfen, index: next });
      }
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  });

  if (playbook.isLoading) return <LoadingSpinner text="定跡を読み込み中..." />;
  if (playbook.error || !tree || !sfen || !view || !playbook.data) {
    return <ErrorMessage message="定跡の取得に失敗しました" onRetry={() => playbook.refetch()} />;
  }

  const isBusy = save.isPending || removeMove.isPending || merge.isPending || split.isPending;
  const toUsisFromRoot = (anchorSfen: string, usis: string[]) => [...pathTo(tree, anchorSfen).map((p) => p.usi), ...usis];

  return (
    <PlaybookResearchTemplate
      playbookName={playbook.data.name}
      tree={tree}
      view={view}
      shape={shape}
      draft={{
        count: draft.count,
        isSaving: save.isPending,
        onSave: () =>
          save.mutate({
            id: playbookId,
            lines: draft.lines.map((l) => ({ usis: toUsisFromRoot(l.anchorSfen, l.nodes.map((n) => n.usi)) })),
          }),
        onDiscard: () => {
          draft.clear();
          if (!contains(tree, sfen)) navigate(tree.rootSfen);
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
      onSelectMove={(index) => setSelected({ sfen, index })}
      onNavigate={navigate}
      onNavigateBoard={(boardId) => {
        const board = tree.boardsById.get(boardId);
        if (board) navigate(board.sfen);
      }}
      onFirst={goFirst}
      onBack={goBack}
      onForward={goForward}
      onMove={play}
      onChangeCondition={(stepId, condition) => updateStep.mutate({ id: playbookId, stepId, condition })}
      onDeleteMove={(usi) => removeMove.mutate({ id: playbookId, sfen, usi })}
      onRename={(name) => updateNode.mutate({ id: playbookId, sfen, name })}
      onSaveMemo={(memo) => updateNode.mutate({ id: playbookId, sfen, memo })}
      onMerge={(otherBoardId) => shape.boardId && merge.mutate({ id: playbookId, boardId: shape.boardId, otherBoardId })}
      onSplit={(boardId) => split.mutate({ id: playbookId, boardId })}
    />
  );
}
