Feature "ログイン・ログアウト" do
  Scenario "ログインページの表示" do
    Given None do
      When "ログインページにアクセスすると、" do
        Then "メールアドレス（またはユーザー名）入力欄が表示される"
          .And "パスワード入力欄が表示される"
          .And "ログインボタンが表示される"
          .And "パスワード再設定へのリンクが表示される"
          .And "新規登録へのリンクが表示される"
      end
    end
  end

  Scenario "正常なログイン" do
    Given "登録済みのユーザーが存在するとき" do
      When "メールアドレスまたはユーザー名と、正しいパスワードを入力してログインすると、" do
        Then "ログイン状態になる"
          .And "トップページに遷移する"
      end
    end

    Given "ログインが必要なページからリダイレクトされてきたとき" do
      When "ログインすると、" do
        Then "元のページに遷移する"
      end
    end
  end

  Scenario "ログイン情報の誤り" do
    Given None do
      When "メールアドレス（またはユーザー名）かパスワードを入力せずにログインしようとすると、" do
        Then "入力を促すエラーメッセージが表示される"
          .And "ログインされない"
      end

      When "存在しないアカウントや誤ったパスワードでログインしようとすると、" do
        # どちらが誤っているかは区別しない
        Then "ユーザー名またはパスワードが正しくない旨のエラーメッセージが表示される"
          .And "ログインされない"
      end
    end
  end

  Scenario "ページ間のリンク" do
    Given None do
      When "パスワード再設定へのリンクをクリックすると、" do
        Then "パスワード再設定ページに遷移する"
      end

      When "新規登録へのリンクをクリックすると、" do
        Then "新規登録ページに遷移する"
      end
    end
  end

  Scenario "ログアウト" do
    Given "ログイン状態のとき" do
      When "ユーザーメニューからログアウトを選ぶと、" do
        Then "ログアウトの確認が表示される"
      end

      When "確認でログアウトを実行すると、" do
        Then "ログアウト状態になる"
          .And "トップページに遷移する"
      end
    end
  end
end
