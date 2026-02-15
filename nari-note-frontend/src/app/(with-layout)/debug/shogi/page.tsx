'use client';

import { ShogiBoard } from '@/lib/next-shogi';

// テスト用のBOD形式盤面データ
const bodText1 = `後手の持駒：飛　金三　銀三　桂二　香四　歩十三
  ９ ８ ７ ６ ５ ４ ３ ２ １
+---------------------------+
| ・ ・ ・ ・ 馬 ・ ・v桂 ・|一
| ・ ・ ・ ・ ・ ・ ・v玉 ・|二
| ・ ・ ・ ・ ・ ・v歩v歩 ・|三
| ・ ・ ・ ・ ・ ・ ・v金v歩|四
| ・ ・ ・ ・ 歩 ・ ・ ・ 歩|五
| ・ ・ ・ ・ ・ ・ ・ ・ ・|六
| ・ ・ ・ ・ ・ ・ ・ ・ ・|七
| ・ ・ ・ ・ ・ ・ ・ ・ ・|八
| ・ ・ ・ ・ ・ ・ ・ ・ ・|九
+---------------------------+
先手の持駒：飛　角　銀　桂`;

// 平手の初期配置
const bodText2 = `後手の持駒：なし
  ９ ８ ７ ６ ５ ４ ３ ２ １
+---------------------------+
|v香|v桂|v銀|v金|v玉|v金|v銀|v桂|v香|一
| ・|v飛| ・| ・| ・| ・| ・|v角| ・|二
|v歩|v歩|v歩|v歩|v歩|v歩|v歩|v歩|v歩|三
| ・| ・| ・| ・| ・| ・| ・| ・| ・|四
| ・| ・| ・| ・| ・| ・| ・| ・| ・|五
| ・| ・| ・| ・| ・| ・| ・| ・| ・|六
| 歩| 歩| 歩| 歩| 歩| 歩| 歩| 歩| 歩|七
| ・| 角| ・| ・| ・| ・| ・| 飛| ・|八
| 香| 桂| 銀| 金| 玉| 金| 銀| 桂| 香|九
+---------------------------+
先手の持駒：なし`;

export default function ShogiDebugPage() {
  return (
    <div className="container mx-auto py-8 px-4">
      <h1 className="text-3xl font-bold mb-8 text-gray-900">
        将棋盤面レンダリングテスト
      </h1>

      <div className="space-y-12">
        {/* テスト1: サイズバリエーション */}
        <section>
          <h2 className="text-2xl font-bold mb-4 text-gray-800">
            サイズバリエーション
          </h2>

          <div className="mb-6">
            <h3 className="text-lg font-semibold mb-2 text-gray-700">Small</h3>
            <ShogiBoard bodText={bodText1} size="sm" />
          </div>

          <div className="mb-6">
            <h3 className="text-lg font-semibold mb-2 text-gray-700">Medium (デフォルト)</h3>
            <ShogiBoard bodText={bodText1} size="md" />
          </div>

          <div className="mb-6">
            <h3 className="text-lg font-semibold mb-2 text-gray-700">Large</h3>
            <ShogiBoard bodText={bodText1} size="lg" />
          </div>
        </section>

        {/* テスト2: 平手初期配置 */}
        <section>
          <h2 className="text-2xl font-bold mb-4 text-gray-800">
            平手初期配置
          </h2>
          <ShogiBoard bodText={bodText2} />
        </section>

        {/* テスト3: オプション */}
        <section>
          <h2 className="text-2xl font-bold mb-4 text-gray-800">
            表示オプション
          </h2>

          <div className="mb-6">
            <h3 className="text-lg font-semibold mb-2 text-gray-700">
              持駒なし
            </h3>
            <ShogiBoard bodText={bodText1} showCapturedPieces={false} />
          </div>

          <div className="mb-6">
            <h3 className="text-lg font-semibold mb-2 text-gray-700">
              プレイヤー名なし
            </h3>
            <ShogiBoard bodText={bodText1} showPlayerNames={false} />
          </div>
        </section>

        {/* テスト4: Markdownでの表示 */}
        <section>
          <h2 className="text-2xl font-bold mb-4 text-gray-800">
            Markdownコードブロックでの表示
          </h2>
          <div className="mb-4 p-4 bg-gray-50 rounded">
            <p className="text-sm text-gray-600 mb-2">
              記事内でBOD形式を使用する場合、以下のように```bodコードブロックで囲みます：
            </p>
            <pre className="bg-gray-900 text-gray-100 p-4 rounded text-xs overflow-x-auto">
{`\`\`\`bod
後手の持駒：飛　金三　銀三　桂二　香四　歩十三
  ９ ８ ７ ６ ５ ４ ３ ２ １
+---------------------------+
| ・ ・ ・ ・ 馬 ・ ・v桂 ・|一
| ・ ・ ・ ・ ・ ・ ・v玉 ・|二
先手の持駒：飛　角　銀　桂
\`\`\``}
            </pre>
          </div>
          <p className="text-sm text-gray-600 mb-4">
            実際のレンダリング結果：
          </p>
          <div className="border-2 border-blue-200 p-4 rounded bg-white">
            <ShogiBoard bodText={bodText1} />
          </div>
        </section>
      </div>
    </div>
  );
}
