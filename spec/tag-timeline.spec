Feature "タグタイムライン機能" do
  Scenario "タグページの基本表示" do
    Given None do
      When "タグページにアクセスすると、" do
        Then "タグ名が表示される"
          .And "タグの説明が表示される"
          .And "タグが使われている記事の総数が表示される"
          .And "タグをフォローしているユーザー数が表示される"
          .And "そのタグを含む記事の一覧が表示される"
      end
    end
  end

  Scenario "記事一覧の表示" do
    Given "タグページにアクセスしているとき" do
      When None do
        Then "各記事のタイトルが表示される"
          .And "各記事の著者名とアイコンが表示される"
          .And "各記事の投稿日時が表示される"
          .And "各記事のいいね数が表示される"
          .And "各記事のコメント数が表示される"
      end
    end
  end

  Scenario "記事のソート順" do
    Given "タグページにアクセスしているとき" do
      When None do
        Then "新着順に記事が表示される"
      end
    end
  end

  Scenario "記事のクリック" do
    Given None do
      When "記事のタイトルをクリックすると、" do
        Then "その記事の詳細ページに遷移する"
      end

      When "記事の著者名またはアイコンをクリックすると、" do
        Then "その著者のプロフィールページに遷移する"
      end
    end
  end

  Scenario "ページネーション" do
    Given "タグを含む記事が20件以上存在するとき" do
      When "タグページにアクセスすると、" do
        Then "記事は20件ずつ表示される"
          .And "ページネーションが表示される"
      end

      When "次のページをクリックすると、" do
        Then "次の20件の記事が表示される"
      end
    end
  end

  Scenario "タグの記事が存在しない場合" do
    Given "タグを含む記事が存在しないとき" do
      When "タグページにアクセスすると、" do
        Then "記事がない旨のメッセージが表示される"
      end
    end
  end
end
