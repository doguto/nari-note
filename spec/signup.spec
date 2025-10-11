Feature "サインアップ機能" do
  Scenario "サインアップページの表示" do
    Given None do
      When "サインアップページにアクセスすると、" do
        Then "ユーザー名入力欄が表示される"
          .And "メールアドレス入力欄が表示される"
          .And "パスワード入力欄が表示される"
          .And "パスワード確認入力欄が表示される"
          .And "サインアップボタンが表示される"
          .And "Googleでサインアップボタンが表示される"
          .And "サインインへのリンクが表示される"
      end
    end
  end

  Scenario "正常なサインアップ" do
    Given None do
      When "すべての必須項目を正しく入力してサインアップボタンを押すと、" do
        Then "新規アカウントが作成される"
          .And "ログイン状態になる"
          .And "ホームページに遷移する"
          .And "サインアップ成功のメッセージが表示される"
      end
    end
  end

  Scenario "Googleでサインアップ" do
    Given None do
      When "Googleでサインアップボタンを押すと、" do
        Then "Googleの認証画面に遷移する"
      end
    end

    Given "Googleの認証画面で認証を完了したとき" do
      Given "初めてGoogleでサインアップするとき" do
        When "認証が成功すると、" do
          Then "新規アカウントが作成される"
            .And "ログイン状態になる"
            .And "ホームページに遷移する"
            .And "サインアップ成功のメッセージが表示される"
        end
      end

      Given "以前にGoogleでサインアップしたことがあるとき" do
        When "認証が成功すると、" do
          Then "既存のアカウントでログイン状態になる"
            .And "ホームページに遷移する"
            .And "サインイン成功のメッセージが表示される"
        end
      end
    end

    Given "Googleの認証画面で認証をキャンセルしたとき" do
      When "キャンセルボタンを押すと、" do
        Then "サインアップページに戻る"
          .And "エラーメッセージは表示されない"
      end
    end

    Given "Googleの認証に失敗したとき" do
      When "エラーが発生すると、" do
        Then "サインアップページに戻る"
          .And "認証失敗のエラーメッセージが表示される"
      end
    end
  end

  Scenario "ユーザー名が未入力" do
    Given None do
      When "ユーザー名を入力せずにサインアップボタンを押すと、" do
        Then "ユーザー名が必須であるエラーメッセージが表示される"
          .And "サインアップが実行されない"
      end
    end
  end

  Scenario "メールアドレスが未入力" do
    Given None do
      When "メールアドレスを入力せずにサインアップボタンを押すと、" do
        Then "メールアドレスが必須であるエラーメッセージが表示される"
          .And "サインアップが実行されない"
      end
    end
  end

  Scenario "パスワードが未入力" do
    Given None do
      When "パスワードを入力せずにサインアップボタンを押すと、" do
        Then "パスワードが必須であるエラーメッセージが表示される"
          .And "サインアップが実行されない"
      end
    end
  end

  Scenario "メールアドレスの形式が不正" do
    Given None do
      When "不正な形式のメールアドレスでサインアップを試みると、" do
        Then "メールアドレスの形式が正しくないエラーメッセージが表示される"
          .And "サインアップが実行されない"
      end
    end
  end

  Scenario "パスワードが短すぎる" do
    Given None do
      When "8文字未満のパスワードでサインアップを試みると、" do
        Then "パスワードは8文字以上である必要があるエラーメッセージが表示される"
          .And "サインアップが実行されない"
      end
    end
  end

  Scenario "パスワードと確認用パスワードが一致しない" do
    Given None do
      When "パスワードと確認用パスワードに異なる値を入力してサインアップを試みると、" do
        Then "パスワードが一致しないエラーメッセージが表示される"
          .And "サインアップが実行されない"
      end
    end
  end

  Scenario "既に登録されているメールアドレス" do
    Given "既に登録されているメールアドレスが存在するとき" do
      When "そのメールアドレスでサインアップを試みると、" do
        Then "このメールアドレスは既に使用されているエラーメッセージが表示される"
          .And "サインアップが実行されない"
      end
    end
  end

  Scenario "既に登録されているユーザー名" do
    Given "既に登録されているユーザー名が存在するとき" do
      When "そのユーザー名でサインアップを試みると、" do
        Then "このユーザー名は既に使用されているエラーメッセージが表示される"
          .And "サインアップが実行されない"
      end
    end
  end

  Scenario "サインインへのリンク" do
    Given None do
      When "サインインへのリンクをクリックすると、" do
        Then "サインインページに遷移する"
      end
    end
  end

  Scenario "ログイン状態での再アクセス" do
    Given "すでにログイン状態のとき" do
      When "サインアップページにアクセスすると、" do
        Then "ホームページにリダイレクトされる"
      end
    end
  end
end
