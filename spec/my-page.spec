Feature "マイページ" do
  # 注: フォロー機能はデータモデルのみ実装済みで、API・UIは未実装です
  Scenario "サインインの要求" do
    Given "未ログイン状態のとき" do
      When "マイページにアクセスすると、" do
        Then "サインインページにリダイレクトされる"
      end
    end
  end

  Scenario "マイページの基本表示" do
    Given None do
      When None do
        # タグの割合とかも表示させたいが、MVPでは省略
        Then "ユーザー名が表示される"
          .And "ユーザーのアイコンが表示される"
          .And "自己紹介文が表示される"
          .And "フォロー・フォロワー数が表示される"
          .And "記事一覧が表示される"
      end
    end
  end

  Scenario "フォロー機能" do
    Given None do
      When "フォロー数の部分をクリックすると、" do
        Then "フォロー一覧のモーダルが表示される"
      end

      When "フォロワー数の部分をクリックすると、" do
        Then "フォロワー一覧のモーダルが表示される"
      end
    end
  end

  Scenario "自己紹介文の編集" do
    Given None do
      When "自己紹介文の編集ボタンを押すと、" do
        Then "自己紹介文編集ページに遷移する"
      end
    end
  end

  Scenario "通知機能" do
    Given None do
      When "通知アイコンをクリックすると、" do
        Then "通知一覧が表示される"
      end
    end

    Given "未読の通知があるとき" do
      When None do
        Then "通知アイコンに赤いバッジが表示される"
      end
    end
  end
end
