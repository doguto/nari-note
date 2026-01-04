Feature "検索機能" do
  # 注: 検索機能は未実装です
  Scenario "記事の検索" do
    Given None do
      When "検索ボックスにキーワードを入力して検索すると、" do
        Then "タイトルまたは本文にそのキーワードを含む記事が表示される"
          .And "各記事のタイトル、著者名、投稿日、タグ、いいね数が表示される"
          .And "検索結果の件数が表示される"
      end

      When "検索キーワードを変更すると、" do
        Then "リアルタイムで検索結果が更新される"
      end

      When "検索ボックスを空にすると、" do
        Then "検索結果がクリアされる"
      end
    end
  end

  Scenario "検索結果のソート" do
    Given "記事の検索結果が表示されているとき" do
      When None do
        Then "投稿日時が新しい順に記事が表示される"
      end
    end
  end

  Scenario "検索結果のページネーション" do
    Given "検索結果が20件以上存在するとき" do
      When "検索を実行すると、" do
        Then "検索結果は20件ずつ表示される"
          .And "ページネーションが表示される"
      end

      When "次のページをクリックすると、" do
        Then "次の20件の検索結果が表示される"
      end
    end
  end

  Scenario "検索結果が存在しない場合" do
    Given None do
      When "検索条件に一致する結果が存在しないとき" do
        Then "該当する結果がない旨のメッセージが表示される"
          .And "検索キーワードの見直しを促すメッセージが表示される"
      end
    end
  end

  Scenario "サジェスト機能" do
    Given None do
      When "検索ボックスに文字を入力すると、" do
        Then "関連する検索キーワード候補が表示される"
          .And "人気のタグが候補として表示される"
      end

      When "サジェスト候補をクリックすると、" do
        Then "その候補で検索が実行される"
      end
    end
  end

  Scenario "検索結果からの遷移" do
    Given "記事の検索結果が表示されているとき" do
      When "記事のタイトルをクリックすると、" do
        Then "その記事の詳細ページに遷移する"
      end

      When "記事の著者名をクリックすると、" do
        Then "その著者のプロフィールページに遷移する"
      end

      When "記事のタグをクリックすると、" do
        Then "そのタグの記事一覧ページに遷移する"
      end
    end
  end
end
