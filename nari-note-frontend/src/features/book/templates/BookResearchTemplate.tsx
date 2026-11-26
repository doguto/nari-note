'use client';

import type { ReactNode } from 'react';
import Link from 'next/link';
import { ArrowRight, ChevronRight, Network, Save, Shapes, Undo2, Zap } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { ErrorAlert } from '@/components/ui';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useMediaQuery } from '@/lib/hooks';
import type { BookMoveDto, EngineLineDto, GetBookPositionResponse } from '@/lib/api/types';
import type { AnalysisEngineId } from '../utils/analysisEngine';
import {
  type BookMoveChange,
  BoardCard,
  BookGraph,
  EngineLinesPanel,
  NextMovesPanel,
  ResearchMemoPanel,
  ShapeHeader,
  ShapeMembersPanel,
  ShapeProposalsPanel,
} from '../organisms';

export type ResearchMove = BookMoveDto & { isDraft?: boolean };

// 盤と「ここからの手」に出す内容。保存していない局面を見ているときは手元で作る
export interface ResearchView {
  sfen: string;
  legalMoves: string[];
  lastMoveUsi?: string | null;
  lastMoveLabel?: string;
  ply: number;
  moves: ResearchMove[];
  isDraft: boolean;
}

interface BookResearchTemplateProps {
  data: GetBookPositionResponse;
  view: ResearchView;
  draft: { count: number; isSaving: boolean; onSave: () => void; onDiscard: () => void };
  analysis: {
    isLoading: boolean;
    isAvailable: boolean;
    hasError: boolean;
    lines: EngineLineDto[];
    engine: AnalysisEngineId;
    onEngineChange: (engine: AnalysisEngineId) => void;
  };
  isBusy: boolean;
  errorMessage?: string;
  selectedMoveIndex: number;
  onSelectMove: (index: number) => void;
  onNavigate: (positionId: number) => void;
  onFirst: () => void;
  onBack: () => void;
  onForward: () => void;
  onMove: (usi: string) => void;
  onMoveChange: (moveId: number, change: BookMoveChange) => void;
  onDeleteMove: (moveId: number) => void;
  onRenameGroup: (name: string) => void;
  onSaveMemo: (memo: string) => void;
  onMerge: (candidateId: number) => void;
  onReject: (candidateId: number) => void;
  onSplit: (positionId: number) => void;
}

const tabListClass = 'grid h-auto w-full rounded-lg bg-brand-bg-gradient-to p-1';
const tabTriggerClass =
  'flex h-9 items-center gap-1.5 rounded-md text-[13px] data-[state=active]:bg-white data-[state=active]:font-bold data-[state=active]:text-brand-text';

function Card({ title, icon, children, className }: { title?: string; icon?: ReactNode; children: ReactNode; className?: string }) {
  return (
    <section className={`flex min-h-0 flex-col gap-3 rounded-xl border border-brand-border bg-white p-3 shadow-sm ${className ?? ''}`}>
      {title && (
        <h2 className="flex items-center gap-2 px-1 text-sm font-bold text-brand-text">
          <span className="text-brand-primary-dark">{icon}</span>
          {title}
        </h2>
      )}
      {children}
    </section>
  );
}

export function BookResearchTemplate(props: BookResearchTemplateProps) {
  const {
    data,
    view,
    draft,
    analysis,
    isBusy,
    errorMessage,
    selectedMoveIndex,
    onSelectMove,
    onNavigate,
    onFirst,
    onBack,
    onForward,
    onMove,
    onMoveChange,
    onDeleteMove,
    onRenameGroup,
    onSaveMemo,
    onMerge,
    onReject,
    onSplit,
  } = props;
  const isWide = useMediaQuery('(min-width: 1024px)');
  const scores = Object.fromEntries(analysis.lines.map((l) => [l.usi, l.score]));

  const shapeHeader = view.isDraft ? (
    <section aria-label="この局面" className="flex items-center gap-2 rounded-xl border border-dashed border-brand-border bg-white px-4 py-3 shadow-sm">
      <span className="text-base font-bold text-brand-secondary-text">保存していない局面</span>
      <span className="text-xs text-brand-secondary-text">保存すると名前を付けられます</span>
    </section>
  ) : (
    <ShapeHeader group={data.group} isBusy={isBusy} onRename={onRenameGroup} onSplit={() => onSplit(data.positionId)} />
  );
  const boardCard = (
    <BoardCard
      sfen={view.sfen}
      legalMoves={view.legalMoves}
      lastMoveUsi={view.lastMoveUsi}
      ply={view.ply}
      lastMoveLabel={view.lastMoveLabel}
      size={isWide ? 'lg' : 'md'}
      disabled={isBusy}
      canBack={view.ply > 0}
      canForward={view.moves.length > 0}
      onMove={onMove}
      onFirst={onFirst}
      onBack={onBack}
      onForward={onForward}
    />
  );
  const nextMoves = (
    <NextMovesPanel
      moves={view.moves}
      scores={scores}
      selectedIndex={selectedMoveIndex}
      className="min-h-0 flex-1"
      onSelect={onSelectMove}
      onGo={onMove}
      onChange={onMoveChange}
      onDelete={onDeleteMove}
    />
  );
  const engine = (
    <EngineLinesPanel
      isLoading={analysis.isLoading}
      isAvailable={analysis.isAvailable}
      hasError={analysis.hasError}
      lines={analysis.lines}
      engine={analysis.engine}
      onEngineChange={analysis.onEngineChange}
      registeredUsis={view.moves.map((m) => m.usi)}
      disabled={isBusy}
      onPlay={onMove}
    />
  );
  const shapeDetails = (
    <div className="flex flex-col gap-5">
      <ShapeProposalsPanel plain proposals={data.proposals} isBusy={isBusy} onView={onNavigate} onMerge={onMerge} onReject={onReject} />
      <ShapeMembersPanel plain members={data.group.members} isBusy={isBusy} onView={onNavigate} onSplit={onSplit} />
      <ResearchMemoPanel plain key={data.group.id} memo={data.group.memo} onSave={onSaveMemo} />
    </div>
  );
  const graph = <BookGraph graph={data.graph} onSelect={onNavigate} className="min-h-0 flex-1" />;
  const shapeTabLabel = data.proposals.length > 0 ? `形 ${data.proposals.length}` : '形';

  const header = (
    <header className="flex flex-wrap items-end justify-between gap-x-6 gap-y-1">
      <div className="min-w-0">
        <nav aria-label="パンくずリスト" className="flex items-center gap-1 text-xs text-brand-secondary-text">
          <Link href="/books" className="hover:text-brand-primary-dark">
            マイ定跡
          </Link>
          <ChevronRight className="h-3.5 w-3.5" aria-hidden="true" />
          <span>研究盤</span>
        </nav>
        <h1 className="truncate text-xl font-bold text-brand-text lg:text-2xl">{data.bookName}</h1>
      </div>
      <div className="flex items-center gap-3">
        {data.pendingJudgeCount > 0 && (
          <p className="text-xs text-brand-secondary-text" aria-live="polite">
            同じ形を判定中（残り {data.pendingJudgeCount} 件）
          </p>
        )}
        {draft.count > 0 && (
          <>
            <span className="text-xs text-brand-secondary-text" aria-live="polite">
              保存していない手 {draft.count}
            </span>
            <Button variant="outline" size="sm" disabled={draft.isSaving} onClick={draft.onDiscard}>
              <Undo2 aria-hidden="true" />
              破棄
            </Button>
          </>
        )}
        <Button size="sm" disabled={draft.count === 0 || draft.isSaving} onClick={draft.onSave}>
          <Save aria-hidden="true" />
          {draft.isSaving ? '保存中...' : '保存'}
        </Button>
      </div>
    </header>
  );

  if (!isWide) {
    return (
      <div className="flex flex-col gap-3 px-3 py-3">
        {header}
        {errorMessage && <ErrorAlert message={errorMessage} />}
        {shapeHeader}
        {boardCard}
        <Tabs defaultValue="moves" className="flex flex-col gap-2">
          <TabsList className={`${tabListClass} grid-cols-4`}>
            <TabsTrigger value="moves" className={`${tabTriggerClass} h-12 flex-col gap-0.5 text-[11px] [&_svg]:size-4`}>
              <ArrowRight className="h-4 w-4" aria-hidden="true" />
              ここからの手
            </TabsTrigger>
            <TabsTrigger value="graph" className={`${tabTriggerClass} h-12 flex-col gap-0.5 text-[11px] [&_svg]:size-4`}>
              <Network className="h-4 w-4" aria-hidden="true" />
              グラフ
            </TabsTrigger>
            <TabsTrigger value="ai" className={`${tabTriggerClass} h-12 flex-col gap-0.5 text-[11px] [&_svg]:size-4`}>
              <Zap className="h-4 w-4" aria-hidden="true" />
              AI候補手
            </TabsTrigger>
            <TabsTrigger value="shape" className={`${tabTriggerClass} h-12 flex-col gap-0.5 text-[11px] [&_svg]:size-4`}>
              <Shapes className="h-4 w-4" aria-hidden="true" />
              {shapeTabLabel}
            </TabsTrigger>
          </TabsList>
          <TabsContent value="moves" className="mt-0">
            {nextMoves}
          </TabsContent>
          <TabsContent value="graph" className="mt-0">
            <Card>
              <BookGraph graph={data.graph} onSelect={onNavigate} className="h-[60dvh]" />
            </Card>
          </TabsContent>
          <TabsContent value="ai" className="mt-0">
            <Card>{engine}</Card>
          </TabsContent>
          <TabsContent value="shape" className="mt-0">
            <Card>{shapeDetails}</Card>
          </TabsContent>
        </Tabs>
      </div>
    );
  }

  return (
    <div className="mx-auto flex w-full max-w-[1440px] flex-col gap-3 px-6 py-4">
      {header}
      {errorMessage && <ErrorAlert message={errorMessage} />}
      <div className="grid grid-cols-[minmax(320px,420px)_auto_minmax(340px,1fr)] items-start gap-4">
        <Card title="定跡グラフ" icon={<Network className="h-4 w-4" aria-hidden="true" />} className="sticky top-4 h-[calc(100dvh-2rem)]">
          {graph}
        </Card>
        <div className="flex flex-col gap-3">
          {shapeHeader}
          {boardCard}
        </div>
        <div className="sticky top-4 flex h-[calc(100dvh-2rem)] min-w-0 flex-col gap-4">
          {nextMoves}
          <Card className="flex-1">
            <Tabs defaultValue="ai" className="flex min-h-0 flex-1 flex-col gap-3">
              <TabsList className={`${tabListClass} grid-cols-2`}>
                <TabsTrigger value="ai" className={tabTriggerClass}>
                  <Zap className="h-4 w-4" aria-hidden="true" />
                  AI候補手
                </TabsTrigger>
                <TabsTrigger value="shape" className={tabTriggerClass}>
                  <Shapes className="h-4 w-4" aria-hidden="true" />
                  {shapeTabLabel}
                </TabsTrigger>
              </TabsList>
              <TabsContent value="ai" className="mt-0 min-h-0 flex-1 overflow-y-auto">
                {engine}
              </TabsContent>
              <TabsContent value="shape" className="mt-0 min-h-0 flex-1 overflow-y-auto">
                {shapeDetails}
              </TabsContent>
            </Tabs>
          </Card>
        </div>
      </div>
    </div>
  );
}
