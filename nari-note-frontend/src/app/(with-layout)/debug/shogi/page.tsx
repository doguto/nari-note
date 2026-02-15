'use client';

import dynamic from 'next/dynamic';

const KifuLite = dynamic(
  () => import('kifu-for-js').then(mod => ({ default: mod.KifuLite })),
  { ssr: false }
);

export default function ShogiDebugPage() {
  return <KifuLite static={true}>
後手の持駒：飛　金三　銀三　桂二　香四　歩十三
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
先手の持駒：飛　角　銀　桂
先手：
後手：
  </KifuLite>
}
