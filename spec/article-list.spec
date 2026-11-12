Feature "自分の記事の管理" do
  Scenario "アクセス制限" do
    Given "未ログイン状態のとき" do
      When "マイ記事一覧ページまたは下書き一覧ページにアクセスすると、" do
        Then "ログインページにリダイレクトされる"
      end
    end
  end

  Scenario "マイ記事一覧の表示" do
    Given "ログイン状態のとき" do
      When "マイ記事一覧ページにアクセスすると、" do
        Then "「公開済み」と「下書き」のタブが件数付きで表示される"
          .And "公開済みタブが選択された状態で表示される"
          .And "新規記事作成ボタンが表示される"
          .And "自分の記事のみが表示される"
      end

      When "公開済みタブでは、" do
        Then "各記事のタイトル、タグ、いいね数、公開日が表示される"
      end

      When "下書きタブでは、" do
        Then "各記事のタイトル、タグ、最終更新日が表示される"
      end
    end

    Given "表示中のタブに記事が存在しないとき" do
      When None do
        Then "記事がない旨のメッセージと新規記事作成ボタンが表示される"
      end
    end
  end

  Scenario "記事の操作" do
    Given "マイ記事一覧に記事が表示されているとき" do
      When "公開済み記事のタイトルをクリックすると、" do
        Then "その記事のページに遷移する"
      end

      When "下書きのタイトルをクリックすると、" do
        Then "その下書きの編集ページに遷移する"
      end

      When "記事のメニューから編集を選ぶと、" do
        Then "その記事の編集ページに遷移する"
      end

      When "新規記事作成ボタンをクリックすると、" do
        Then "新規記事の作成ページに遷移する"
      end
    end
  end

  Scenario "記事の削除" do
    Given "マイ記事一覧に記事が表示されているとき" do
      When "記事のメニューから削除を選ぶと、" do
        Then "削除確認のモーダルが表示される"
      end

      When "削除確認のモーダルで削除を実行すると、" do
        Then "その記事が削除される"
          .And "一覧が更新される"
      end

      When "削除確認のモーダルでキャンセルすると、" do
        Then "モーダルが閉じる"
          .And "記事は削除されない"
      end
    end

    Given "メールアドレスが未認証のとき" do
      When "削除を選ぶと、" do
        Then "メールアドレスの認証を促すモーダルが表示される"
      end
    end
  end

  Scenario "下書き一覧" do
    Given "ログイン状態のとき" do
      When "下書き一覧ページにアクセスすると、" do
        Then "自分の下書きのみが最終更新日時の新しい順に表示される"
          .And "新規記事作成ボタンが表示される"
      end

      When "下書きのメニューから削除を選ぶと、" do
        Then "削除確認が表示され、確定するとその下書きが削除される"
      end
    end

    Given "下書きが存在しないとき" do
      When "下書き一覧ページにアクセスすると、" do
        Then "下書きがない旨のメッセージが表示される"
      end
    end
  end
end
