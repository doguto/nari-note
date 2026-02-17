/**
 * TermsTemplate - Template Component
 *
 * 利用規約ページのUI構成に責任を持つ
 */
export function TermsTemplate() {
  return (
    <div className="max-w-3xl mx-auto bg-white rounded-lg shadow p-8 space-y-8">
      <h1 className="text-2xl font-bold text-brand-text">利用規約</h1>

      <p className="text-sm text-gray-500">最終更新日：2024年1月1日</p>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold text-brand-text">第1条（適用）</h2>
        <p className="text-gray-700 leading-relaxed">
          本利用規約（以下「本規約」といいます）は、なりノート（以下「本サービス」といいます）の利用条件を定めるものです。
          ユーザーの皆さまには、本規約に従って本サービスをご利用いただきます。
        </p>
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold text-brand-text">第2条（利用登録）</h2>
        <p className="text-gray-700 leading-relaxed">
          本サービスへの登録を希望する方は、本規約に同意のうえ、所定の方法により利用登録を申請してください。
          利用登録の完了をもって、ユーザーと当社の間に本規約を内容とする契約が成立するものとします。
        </p>
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold text-brand-text">第3条（禁止事項）</h2>
        <p className="text-gray-700 leading-relaxed">
          ユーザーは、本サービスの利用にあたり、以下の行為をしてはなりません。
        </p>
        <ul className="list-disc list-inside text-gray-700 space-y-1 pl-2">
          <li>法令または公序良俗に違反する行為</li>
          <li>犯罪行為に関連する行為</li>
          <li>他のユーザーへの嫌がらせや誹謗中傷</li>
          <li>他のユーザーの個人情報を無断で収集・掲載する行為</li>
          <li>本サービスの運営を妨げる行為</li>
          <li>不正アクセス行為</li>
          <li>その他、当社が不適切と判断する行為</li>
        </ul>
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold text-brand-text">第4条（コンテンツの権利）</h2>
        <p className="text-gray-700 leading-relaxed">
          ユーザーが本サービスに投稿した記事・コメントその他のコンテンツに関する著作権は、当該ユーザーに帰属します。
          ただし、ユーザーは当社に対し、本サービスの運営・改善・プロモーションに必要な範囲で、無償かつ非独占的に当該コンテンツを利用する権利を許諾するものとします。
        </p>
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold text-brand-text">第5条（サービスの変更・停止）</h2>
        <p className="text-gray-700 leading-relaxed">
          当社は、ユーザーへの事前通知なく、本サービスの内容を変更・中断・終了することができるものとします。
          これによりユーザーに生じた損害について、当社は一切の責任を負いません。
        </p>
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold text-brand-text">第6条（免責事項）</h2>
        <p className="text-gray-700 leading-relaxed">
          当社は、本サービスに関して、ユーザーと他のユーザーまたは第三者との間において生じたトラブルについて一切責任を負いません。
          本サービスに起因してユーザーに生じた損害について、当社は一切の責任を負いません。
        </p>
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold text-brand-text">第7条（規約の変更）</h2>
        <p className="text-gray-700 leading-relaxed">
          当社は、必要と判断した場合には、ユーザーへの事前通知なく、いつでも本規約を変更することができるものとします。
          変更後の利用規約は、本サービス上に掲示した時点から効力を生じるものとします。
        </p>
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold text-brand-text">第8条（準拠法・管轄裁判所）</h2>
        <p className="text-gray-700 leading-relaxed">
          本規約の解釈にあたっては、日本法を準拠法とします。
          本サービスに関して紛争が生じた場合には、当社の本店所在地を管轄する裁判所を専属的合意管轄とします。
        </p>
      </section>
    </div>
  );
}
