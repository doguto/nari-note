Feature "プロフィールページ" do
  Scenario "プロフィールの表示" do
    Given None do
      When "ユーザーのプロフィールページにアクセスすると、" do
        Then "ユーザー名とアイコンが表示される"
          .And "自己紹介文が設定されていれば表示される"
          .And "記事数、フォロワー数、フォロー中の数が表示される"
          .And "「記事」「講座」「いいね」「フォロー中のタグ」のタブが表示される"
          .And "記事タブが選択された状態で表示される"
      end
    end
  end

  Scenario "コンテンツタブ" do
    Given None do
      When "記事タブを選択すると、" do
        Then "そのユーザーの公開済み記事が表示される"
      end

      When "講座タブを選択すると、" do
        Then "そのユーザーの公開済み講座が表示される"
      end

      When "いいねタブを選択すると、" do
        Then "そのユーザーがいいねした記事が表示される"
      end

      When "フォロー中のタグタブを選択すると、" do
        Then "「フォロー中のタグ一覧は今後実装予定です」と表示される"
      end

      When "タブを切り替えると、" do
        Then "選択中のタブとページ番号が URL に反映される"
      end
    end

    Given "一覧の件数が1ページの表示件数を超えるとき" do
      When None do
        Then "ページネーションが表示される"
      end
    end
  end

  Scenario "フォロワー・フォロー中の一覧" do
    Given None do
      When "フォロワー数をクリックすると、" do
        Then "フォロワー一覧のタブが表示される"
      end

      When "フォロー中の数をクリックすると、" do
        Then "フォロー中のユーザー一覧のタブが表示される"
      end

      When "記事数をクリックすると、" do
        Then "記事タブに戻る"
      end
    end
  end

  Scenario "フォロー" do
    Given "他のユーザーのプロフィールを表示しているとき" do
      Given "フォローしていないとき" do
        When "フォローボタンを押すと、" do
          Then "ユーザーをフォローする"
            .And "ボタンの表示がフォロー解除に変わる"
            .And "フォロワー数が1増える"
        end
      end

      Given "フォローしているとき" do
        When "フォローボタンを押すと、" do
          Then "フォローを解除する"
            .And "ボタンの表示が元に戻る"
            .And "フォロワー数が1減る"
        end
      end

      Given "未ログイン状態のとき" do
        When "フォローボタンを押すと、" do
          Then "ログインを促すモーダルが表示される"
        end
      end

      Given "メールアドレスが未認証のとき" do
        When "フォローボタンを押すと、" do
          Then "メールアドレスの認証を促すモーダルが表示される"
        end
      end
    end
  end

  Scenario "自分のプロフィール" do
    Given "自分のプロフィールを表示しているとき" do
      When None do
        Then "フォローボタンの代わりにプロフィール編集ボタンが表示される"
      end

      When "プロフィール編集ボタンを押すと、" do
        Then "プロフィール編集ページに遷移する"
      end
    end
  end
end
