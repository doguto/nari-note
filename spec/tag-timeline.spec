Feature "タグページ" do
  Scenario "タグページの基本表示" do
    Given None do
      When "タグページにアクセスすると、" do
        Then "タグ名が表示される"
          .And "「記事」と「講座」のタブが表示される"
          .And "表示中のタブの件数が表示される"
      end
    end
  end

  Scenario "記事タブ" do
    Given "記事タブを表示しているとき" do
      When None do
        Then "そのタグが付いた公開済みの単体記事が公開日時の新しい順に表示される"
          .And "各記事のタイトル、著者、タグ、いいね数、投稿日が表示される"
      end
    end
  end

  Scenario "講座タブ" do
    Given "講座タブを表示しているとき" do
      When None do
        Then "そのタグが付いた記事を含む公開済みの講座が表示される"
      end
    end
  end

  Scenario "一覧からの遷移" do
    Given None do
      When "記事をクリックすると、" do
        Then "その記事のページに遷移する"
      end

      When "講座をクリックすると、" do
        Then "その講座のページに遷移する"
      end

      When "著者をクリックすると、" do
        Then "その著者のプロフィールページに遷移する"
      end
    end
  end

  Scenario "ページネーション" do
    Given "表示中のタブの件数が1ページの表示件数を超えるとき" do
      When None do
        Then "ページネーションが表示される"
      end

      When "次のページをクリックすると、" do
        Then "次の一覧が表示される"
          .And "ページ番号とタブが URL に反映される"
      end
    end
  end

  Scenario "該当がない場合" do
    Given "タグに該当する記事・講座が存在しないとき" do
      When "タグページにアクセスすると、" do
        Then "記事（講座）がない旨のメッセージが表示される"
      end
    end
  end
end
