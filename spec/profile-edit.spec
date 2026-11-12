Feature "プロフィール編集" do
  Scenario "アクセス制限" do
    Given "未ログイン状態のとき" do
      When "プロフィール編集ページにアクセスすると、" do
        Then "ログインページにリダイレクトされる"
      end
    end
  end

  Scenario "プロフィール編集ページの表示" do
    Given "ログイン状態のとき" do
      When "プロフィール編集ページにアクセスすると、" do
        Then "設定メニューが表示される"
          .And "プロフィール画像のアップロード欄が表示される"
          .And "現在のユーザー名と自己紹介文が入力された状態で表示される"
          .And "保存ボタンとキャンセルボタンが表示される"
      end
    end

    Given "変更が無いとき" do
      When None do
        Then "保存ボタンは押せない"
      end
    end
  end

  Scenario "プロフィールの保存" do
    Given "ユーザー名・自己紹介文・プロフィール画像のいずれかを変更したとき" do
      When "保存ボタンを押すと、" do
        Then "プロフィールが更新される"
          .And "自分のプロフィールページに遷移する"
      end
    end

    Given "自己紹介文を空にしたとき" do
      When "保存ボタンを押すと、" do
        Then "自己紹介文が空の状態で更新される"
      end
    end
  end

  Scenario "ユーザー名のバリデーション" do
    # 文字数・使用可能文字の正は ProfileEditPage と UpdateUserProfileRequest
    Given "プロフィール編集ページを表示しているとき" do
      When "ユーザー名を空欄にして保存しようとすると、" do
        Then "ユーザー名が必須である旨のエラーメッセージが表示される"
          .And "保存されない"
      end

      When "文字数や使用できない文字の条件を満たさないユーザー名で保存しようとすると、" do
        Then "条件を示すエラーメッセージが表示される"
          .And "保存されない"
      end

      When "他のユーザーが使用中のユーザー名で保存しようとすると、" do
        Then "既に使用されている旨のエラーメッセージが表示される"
          .And "保存されない"
      end
    end
  end

  Scenario "自己紹介文のバリデーション" do
    Given "プロフィール編集ページを表示しているとき" do
      When "自己紹介文が上限文字数を超えて保存しようとすると、" do
        Then "上限文字数を示すエラーメッセージが表示される"
          .And "保存されない"
      end
    end
  end

  Scenario "プロフィール画像" do
    Given "プロフィール編集ページを表示しているとき" do
      When "画像ファイルを選択すると、" do
        Then "プレビューが表示される"
          .And "保存時にアップロードされる"
      end

      When "選択した画像を取り消すと、" do
        Then "プレビューが消え、画像は変更されない"
      end

      When "画像以外のファイルや上限サイズを超える画像を選択すると、" do
        Then "エラーメッセージが表示される"
          .And "画像は選択されない"
      end
    end

    Given "メールアドレスが未認証のとき" do
      When "画像を選択して保存しようとすると、" do
        Then "メールアドレスの認証を促すモーダルが表示される"
          .And "保存されない"
      end
    end
  end

  Scenario "編集のキャンセル" do
    Given "未保存の変更があるとき" do
      When "キャンセルボタンを押すと、" do
        Then "変更破棄の確認が表示される"
      end

      When "確認で破棄を選ぶと、" do
        Then "変更が破棄される"
          .And "自分のプロフィールページに遷移する"
      end

      When "確認でキャンセルを選ぶと、" do
        Then "確認が閉じ、編集ページに留まる"
      end
    end

    Given "変更が無いとき" do
      When "キャンセルボタンを押すと、" do
        Then "確認なしで自分のプロフィールページに遷移する"
      end
    end
  end
end
