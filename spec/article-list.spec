Feature "記事一覧機能" do
  Scenario "記事一覧ページの表示" do
    Given "ログイン状態のとき" do
      When "記事一覧ページにアクセスすると、" do
        Then "下書きタブにアクセスし、自分の下書き記事の一覧が表示される"
          .And "各下書きのタイトルが表示される"
          .And "各下書きの最終更新日時が表示される"
          .And "各下書きの編集ボタンが表示される"
          .And "各下書きの削除ボタンが表示される"
          .And "新規記事作成ボタンが表示される"
          .And "投稿済み記事タブが表示される"
      end

      Given "下書きが存在しないとき" do
        When "記事一覧ページの下書きタブにアクセスすると、" do
          Then "下書きがない旨のメッセージが表示される"
            .And "新規記事作成ボタンが表示される"
        end
      end
    end
  end

  Scenario "下書きの並び順" do
    Given "ログイン状態で複数の下書きが存在するとき" do
      When "下書き一覧ページにアクセスすると、" do
        Then "下書きは最終更新日時が新しい順に表示される"
      end
    end
  end

  Scenario "下書きの編集" do
    Given "ログイン状態で下書きが存在するとき" do
      When "下書きの編集ボタンをクリックすると、" do
        Then "その下書きの編集ページに遷移する"
      end
    end
  end

  Scenario "下書きのタイトルクリック" do
    Given "ログイン状態で下書きが存在するとき" do
      When "下書きのタイトルをクリックすると、" do
        Then "その下書きの編集ページに遷移する"
      end
    end
  end

  Scenario "下書きの削除" do
    Given "ログイン状態で下書きが存在するとき" do
      When "下書きの削除ボタンをクリックすると、" do
        Then "削除確認のモーダルが表示される"
      end

      When "削除確認のモーダルで削除を実行すると、" do
        Then "その下書きが削除される"
          .And "下書き一覧ページが更新される"
          .And "削除成功のメッセージが表示される"
      end

      When "削除確認のモーダルでキャンセルすると、" do
        Then "モーダルが閉じる"
          .And "下書きは削除されない"
      end
    end
  end

  Scenario "新規記事作成" do
    Given "ログイン状態のとき" do
      When "新規記事作成ボタンをクリックすると、" do
        Then "新規記事の編集ページに遷移する"
      end
    end
  end

  Scenario "下書きの検索" do
    Given "ログイン状態で複数の下書きが存在するとき" do
      When "検索欄にキーワードを入力すると、" do
        Then "タイトルまたは本文にそのキーワードを含む下書きのみが表示される"
      end

      When "検索欄を空にすると、" do
        Then "すべての下書きが表示される"
      end
    end
  end

  Scenario "ページネーション" do
    Given "ログイン状態で下書きが20件以上存在するとき" do
      When "下書き一覧ページにアクセスすると、" do
        Then "下書きは20件ずつ表示される"
          .And "ページネーションが表示される"
      end

      When "次のページをクリックすると、" do
        Then "次の20件の下書きが表示される"
      end
    end
  end

  Scenario "未ログイン状態でのアクセス" do
    Given "未ログイン状態のとき" do
      When "下書き一覧ページにアクセスすると、" do
        Then "サインインページにリダイレクトされる"
      end
    end
  end

  Scenario "他人の下書きは表示されない" do
    Given "ログイン状態で他のユーザーの下書きが存在するとき" do
      When "下書き一覧ページにアクセスすると、" do
        Then "自分の下書きのみが表示される"
          .And "他のユーザーの下書きは表示されない"
      end
    end
  end

  Scenario "投稿済み記事の編集" do
    Given "投稿済み記事タブがアクティブなとき" do
      When "投稿済み記事の編集ボタンをクリックすると、" do
        Then "その投稿済み記事の編集ページに遷移する"
      end
    end
  end
end
