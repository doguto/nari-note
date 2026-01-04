Feature "プロフィールページ" do
  # 注: フォロー機能はデータモデルのみ実装済みで、API・UIは未実装です
  Scenario "プロフィールページの基本表示" do
    Given None do
      When None do
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

    Given "フォローをしていないとき" do
      When None do
        Then "フォローボタンが表示される"
      end

      When "フォローボタンを押すと、" do
        Then "ユーザーをフォローする"
          .And "フォローボタンの表示が変わる"
          .And "フォロー数が1増える"
      end
    end

    Given "フォローをしているとき" do
      When None do
        Then "フォロー解除ボタンが表示される"
      end

      When "フォローボタンを押すと、" do
        Then "ユーザーのフォローを解除する"
          .And "フォローボタンの表示が元に戻る"
          .And "フォロー数が1減る"
      end
    end
  end
end
