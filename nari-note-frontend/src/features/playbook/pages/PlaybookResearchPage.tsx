'use client';

import { useEffect, useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { ErrorMessage, LoadingSpinner } from '@/components/ui';
import {
  playbooksApi,
  useDeletePlaybookNode,
  useDeletePlaybookTransition,
  useGetPlaybook,
  useInspectPlaybookBoard,
  useMergePlaybookNodes,
  useSavePlaybookLines,
  useSplitPlaybookBoard,
  useUnlinkPlaybookNode,
  useUpdatePlaybookNode,
  useUpdatePlaybookTransition,
} from '@/lib/api';
import { useAnalysisEngine } from '../hooks/useAnalysisEngine';
import { useResearchDraft } from '../hooks/useResearchDraft';
import type { ResearchMove } from '../organisms';
import { type ResearchShape, type ResearchView, PlaybookResearchTemplate } from '../templates';
import { buildPlaybookTree, contains, edgeThrough, pathTo } from '../utils/playbookTree';
import { destinationOf, legalMoves, moveLabel, play as playOn } from '../utils/shogiRules';

interface PlaybookResearchPageProps {
  playbookId: string;
}

interface PendingDelete {
  title: string;
  description: string;
  run: () => void;
}

function isTyping(target: EventTarget | null): boolean {
  return target instanceof HTMLElement && (target.isContentEditable || ['INPUT', 'TEXTAREA', 'SELECT'].includes(target.tagName));
}

// 定跡の全体を 1 回取得し、手元で局面を行き来する。開くと常に開始局面から始まる
export function PlaybookResearchPage({ playbookId }: PlaybookResearchPageProps) {
  const [errorMessage, setErrorMessage] = useState<string>();
  const [selected, setSelected] = useState<{ sfen: string; index: number } | null>(null);
  const [current, setCurrent] = useState<string | null>(null);
  const [pendingDelete, setPendingDelete] = useState<PendingDelete | null>(null);
  const draft = useResearchDraft();
  const [engine, setEngine] = useAnalysisEngine();

  const playbook = useGetPlaybook({ id: playbookId });
  const tree = useMemo(() => (playbook.data ? buildPlaybookTree(playbook.data) : null), [playbook.data]);

  const sfen = useMemo(() => {
    if (!tree) return null;
    if (current && (contains(tree, current) || draft.get(current))) return current;
    return tree.rootSfen;
  }, [tree, current, draft]);

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
  };
  const onError = (message: string) => () => setErrorMessage(message);
  const closeDialog = { onSettled: () => setPendingDelete(null) };

  const save = useSavePlaybookLines({
    // 保存した局面が定跡の取得し直しで現れるまで、下書きを残して盤を保つ
    onSuccess: async () => {
      await playbook.refetch();
      draft.clear();
    },
    onError: onError('保存できませんでした。'),
  });
  const updateNode = useUpdatePlaybookNode({ onError: onError('保存できませんでした。') });
  const updateTransition = useUpdatePlaybookTransition({ onError: onError('保存できませんでした。') });
  const merge = useMergePlaybookNodes({ onError: onError('まとめられませんでした。') });
  const split = useSplitPlaybookBoard({ onError: onError('分けられませんでした。') });
  const unlink = useUnlinkPlaybookNode({ onError: onError('ノードを解除できませんでした。分岐点や合流点のノードは解除できません。') });
  const deleteNode = useDeletePlaybookNode({ ...closeDialog, onError: onError('ノードを削除できませんでした。') });
  const deleteTransition = useDeletePlaybookTransition({ ...closeDialog, onError: onError('辺を削除できませんでした。') });

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

  const board = sfen ? tree?.boards.get(sfen) : undefined;
  const nodeId = board?.nodeId;

  const view = useMemo((): ResearchView | null => {
    if (!tree || !sfen) return null;
    const last = path[path.length - 1];
    const previousTo = last ? destinationOf(last.usi) : undefined;
    const suggestions = new Map((inspection.data?.conditionSuggestions ?? []).map((c) => [c.transitionId, c.suggestions]));
    // ノードでない局面では保存済みの手を出さない。辺を進むのは → キーで行う
    const saved: ResearchMove[] =
      nodeId !== undefined
        ? (tree.children.get(sfen) ?? []).map((m) => ({
            usi: m.usi,
            label: moveLabel(sfen, m.usi, previousTo),
            isDraft: false,
            transitionId: m.transition.id,
            condition: m.transition.condition,
            conditionSuggestions: suggestions.get(m.transition.id) ?? [],
          }))
        : [];
    const savedUsis = new Set((tree.children.get(sfen) ?? []).map((m) => m.usi));
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
  }, [tree, sfen, nodeId, path, draft, inspection.data]);

  const shape = useMemo((): ResearchShape => {
    const node = nodeId !== undefined ? tree?.nodes.get(nodeId) : undefined;
    const boardCount = node && tree ? [...tree.boards.values()].filter((b) => b.nodeId === node.id).length : 0;
    const info = inspection.data?.boardId === board?.id ? inspection.data : undefined;
    return {
      isNode: !!node,
      isRoot: sfen === tree?.rootSfen,
      name: node?.name ?? '',
      memo: node?.memo ?? '',
      boardCount,
      nameSuggestions: inspection.data?.nameSuggestions ?? [],
      members: info?.members ?? [],
      similar: info?.similar ?? [],
      boardId: board?.id,
    };
  }, [tree, sfen, board, nodeId, inspection.data]);

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
  // ノードでない局面では保存済みの手を一覧に出さないので、次の遷移で進む
  const goForward = () => {
    if (!tree || !sfen) return;
    const move = view?.moves[selectedIndex] ?? tree.children.get(sfen)?.[0] ?? view?.moves[0];
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

  const isBusy =
    save.isPending || merge.isPending || split.isPending || unlink.isPending || deleteNode.isPending || deleteTransition.isPending;
  const toUsisFromRoot = (anchorSfen: string, usis: string[]) => [...pathTo(tree, anchorSfen).map((p) => p.usi), ...usis];
  const confirmDeleteTransition = (transitionId: number) =>
    setPendingDelete({
      title: 'この辺を削除しますか？',
      description: '辺の手順と、その先にしか無いノード・手順を削除します。この操作は取り消せません。',
      run: () => deleteTransition.mutate({ id: playbookId, transitionId }),
    });

  return (
    <>
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
          const target = tree.boardsById.get(boardId);
          if (target) navigate(target.sfen);
        }}
        onFirst={goFirst}
        onBack={goBack}
        onForward={goForward}
        onMove={play}
        onChangeCondition={(transitionId, condition) => updateTransition.mutate({ id: playbookId, transitionId, condition })}
        onDeleteTransition={confirmDeleteTransition}
        onDeleteTransitionHere={() => {
          const transition = edgeThrough(tree, sfen);
          if (transition) confirmDeleteTransition(transition.id);
        }}
        onMakeNode={() => updateNode.mutate({ id: playbookId, sfen })}
        onUnlinkNode={() => nodeId !== undefined && unlink.mutate({ id: playbookId, nodeId })}
        onDeleteNode={() =>
          nodeId !== undefined &&
          setPendingDelete({
            title: 'このノードを削除しますか？',
            description: 'このノードと、その先にしか無いノード・手順を削除します。この操作は取り消せません。',
            run: () => deleteNode.mutate({ id: playbookId, nodeId }),
          })
        }
        onRename={(name) => updateNode.mutate({ id: playbookId, sfen, name })}
        onSaveMemo={(memo) => updateNode.mutate({ id: playbookId, sfen, memo })}
        onMerge={(otherBoardId) => shape.boardId && merge.mutate({ id: playbookId, boardId: shape.boardId, otherBoardId })}
        onSplit={(boardId) => split.mutate({ id: playbookId, boardId })}
      />
      <Dialog open={pendingDelete !== null} onOpenChange={(open) => !open && setPendingDelete(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{pendingDelete?.title}</DialogTitle>
            <DialogDescription>{pendingDelete?.description}</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setPendingDelete(null)}>
              キャンセル
            </Button>
            <Button disabled={isBusy} className="bg-red-500 text-white hover:bg-red-600" onClick={() => pendingDelete?.run()}>
              削除する
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
