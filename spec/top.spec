Feature "トップページ" do
  Scenario "トップタイムラインの基本表示" do
    Given None do
      When None do
        # 記事一覧のソート順は実装の簡易化のため、新着順を想定
        # 人気順やユーザーの好みに合わせたタイムラインとかも表示させたいが、MVPでは省略
        # 全部の記事を表示させたいが、パフォーマンスを考えてキャッシュしてある記事の範囲で
        Then "記事一覧が表示される"
          .And "各記事のタイトル、著者名、投稿日、タグ、いいね数が表示される"
          .And "ページネーションが20件単位程度で表示される"
      end

      When "記事をクリックすると、" do
        Then "その記事のページに遷移する"
      end
    end
  end

  Scenario "検索機能" do
    Given None do
      When None do
        Then "検索ボックスが表示される"
          .And "検索ボタンが表示される"
      end

      When "検索ボックスにキーワードを入力して検索すると、" do
        Then "検索結果ページに遷移する"
      end
    end
  end
end
