Feature "サインイン機能" do
  Scenario "サインインページの表示" do
    Given None do
      When "サインインページにアクセスすると、" do
        Then "メールアドレス入力欄が表示される"
          .And "パスワード入力欄が表示される"
          .And "サインインボタンが表示される"
          .And "Googleでサインインボタンが表示される"
          .And "パスワード忘れのリンクが表示される"
          .And "新規登録へのリンクが表示される"
      end
    end
  end

  Scenario "正常なサインイン" do
    Given "登録済みのユーザーが存在するとき" do
      When "正しいメールアドレスとパスワードを入力してサインインボタンを押すと、" do
        Then "ログイン状態になる"
          .And "ホームページに遷移する"
          .And "サインイン成功のメッセージが表示される"
      end
    end
  end

  Scenario "Googleでサインイン" do
    Given None do
      When "Googleでサインインボタンを押すと、" do
        Then "Googleの認証画面に遷移する"
      end
    end

    Given "Googleの認証画面で認証を完了したとき" do
      Given "初めてGoogleでサインインするとき" do
        When "認証が成功すると、" do
          Then "新規アカウントが作成される"
            .And "ログイン状態になる"
            .And "ホームページに遷移する"
            .And "サインイン成功のメッセージが表示される"
        end
      end

      Given "以前にGoogleでサインインしたことがあるとき" do
        When "認証が成功すると、" do
          Then "既存のアカウントでログイン状態になる"
            .And "ホームページに遷移する"
            .And "サインイン成功のメッセージが表示される"
        end
      end
    end

    Given "Googleの認証画面で認証をキャンセルしたとき" do
      When "キャンセルボタンを押すと、" do
        Then "サインインページに戻る"
          .And "エラーメッセージは表示されない"
      end
    end

    Given "Googleの認証に失敗したとき" do
      When "エラーが発生すると、" do
        Then "サインインページに戻る"
          .And "認証失敗のエラーメッセージが表示される"
      end
    end
  end

  Scenario "メールアドレスが未入力" do
    Given None do
      When "メールアドレスを入力せずにサインインボタンを押すと、" do
        Then "メールアドレスが必須であるエラーメッセージが表示される"
          .And "サインインが実行されない"
      end
    end
  end

  Scenario "パスワードが未入力" do
    Given None do
      When "パスワードを入力せずにサインインボタンを押すと、" do
        Then "パスワードが必須であるエラーメッセージが表示される"
          .And "サインインが実行されない"
      end
    end
  end

  Scenario "メールアドレスが不正" do
    Given None do
      When "存在しないメールアドレスでサインインを試みると、" do
        Then "メールアドレスまたはパスワードが正しくないエラーメッセージが表示される"
          .And "サインインが失敗する"
      end
    end
  end

  Scenario "パスワードが不正" do
    Given "登録済みのユーザーが存在するとき" do
      When "正しいメールアドレスと間違ったパスワードでサインインを試みると、" do
        Then "メールアドレスまたはパスワードが正しくないエラーメッセージが表示される"
          .And "サインインが失敗する"
      end
    end
  end

  Scenario "パスワード忘れ" do
    Given None do
      When "パスワード忘れのリンクをクリックすると、" do
        Then "パスワードリセットページに遷移する"
      end
    end
  end

  Scenario "新規登録へのリンク" do
    Given None do
      When "新規登録へのリンクをクリックすると、" do
        Then "サインアップページに遷移する"
      end
    end
  end

  Scenario "ログイン状態での再アクセス" do
    Given "すでにログイン状態のとき" do
      When "サインインページにアクセスすると、" do
        Then "ホームページにリダイレクトされる"
      end
    end
  end
end
