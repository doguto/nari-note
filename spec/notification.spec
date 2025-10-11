Feature "通知機能" do
  Scenario "通知一覧の表示" do
    Given "ログイン状態のとき" do
      When "通知アイコンをクリックすると、" do
        Then "通知一覧のドロップダウンが表示される"
          .And "各通知のアイコンが表示される"
          .And "各通知の内容が表示される"
          .And "各通知の日時が表示される"
      end

      When "通知一覧が表示されているとき" do
        # 直近1カ月以外の通知は無駄なので、データ容量を考えて削除
        Then "新着順で、直近1カ月の通知が表示される"
          .And "最大20件の通知が表示される"
      end
    end
  end

  Scenario "フォローユーザーの投稿通知" do
    Given "フォローしているユーザーが記事を投稿したとき" do
      When None do
        Then "そのユーザーの記事投稿通知が作成される"
          .And "通知アイコンにバッジが表示される"
      end

      When "通知をクリックすると、" do
        Then "投稿された記事のページに遷移する"
          .And "その通知が既読になる"
      end
    end
  end

  Scenario "通知の既読" do
    Given "未読の通知があるとき" do
      When "通知一覧を開くと、" do
        Then "表示された通知が自動的に既読になる"
          .And "バッジの未読数が減る"
      end

      When "すべての通知が既読になると、" do
        Then "バッジが非表示になる"
      end
    end
  end

  Scenario "すべての通知を既読にする" do
    Given "未読の通知が複数あるとき" do
      When "すべて既読にするボタンをクリックすると、" do
        Then "すべての通知が既読になる"
          .And "バッジが非表示になる"
      end
    end
  end

  Scenario "通知が存在しない場合" do
    Given "通知が1件もないとき" do
      When "通知一覧を開くと、" do
        Then "通知がない旨のメッセージが表示される"
      end
    end
  end

  Scenario "通知ページ" do
    Given "ログイン状態のとき" do
      When "通知ページにアクセスすると、" do
        Then "通知が20件ずつ表示される"
          .And "ページネーションが表示される"
      end

      When "次のページをクリックすると、" do
        Then "次の20件の通知が表示される"
      end
    end
  end

  Scenario "未ログイン状態" do
    Given "未ログイン状態のとき" do
      When None do
        Then "通知アイコンは表示されない"
      end

      When "通知ページにアクセスしようとすると、" do
        Then "サインインページにリダイレクトされる"
      end
    end
  end
end
