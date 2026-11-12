Feature "検索" do
  Scenario "検索ページの表示" do
    Given None do
      When "キーワード未指定で検索ページにアクセスすると、" do
        Then "検索ボックスと「記事」「講座」のタブが表示される"
          .And "キーワードの入力を促すメッセージが表示される"
      end
    end
  end

  Scenario "記事の検索" do
    Given None do
      When "キーワードを入力して検索ボタンを押すかEnterキーを押すと、" do
        # 講座に属する記事は講座の検索結果としてヒットさせる
        Then "タイトルまたは本文にキーワードを含む公開済みの単体記事が新しい順に表示される"
          .And "各記事のタイトル、著者、タグ、いいね数、投稿日が表示される"
          .And "検索結果の件数が表示される"
      end
    end
  end

  Scenario "講座の検索" do
    Given "キーワードを指定して検索したとき" do
      When "講座タブを選択すると、" do
        Then "講座名、または講座内の公開済み記事のタイトル・本文にキーワードを含む公開済みの講座が表示される"
          .And "検索結果の件数が表示される"
      end
    end
  end

  Scenario "検索条件の URL 管理" do
    Given None do
      When "検索・タブ切り替え・ページ送りをすると、" do
        Then "キーワード・タブ・ページ番号が URL に反映される"
          .And "ブラウザバックで直前の検索結果に戻れる"
      end

      When "タブを切り替えると、" do
        Then "1ページ目から表示される"
      end
    end
  end

  Scenario "ページネーション" do
    Given "検索結果が1ページの表示件数を超えるとき" do
      When None do
        Then "ページネーションが表示される"
      end

      When "次のページをクリックすると、" do
        Then "次の検索結果が表示される"
      end
    end
  end

  Scenario "検索結果が存在しない場合" do
    Given None do
      When "検索条件に一致する結果が存在しないとき" do
        Then "該当する結果がない旨と、別のキーワードでの検索を促すメッセージが表示される"
      end
    end
  end

  Scenario "検索結果からの遷移" do
    Given "検索結果が表示されているとき" do
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
end
