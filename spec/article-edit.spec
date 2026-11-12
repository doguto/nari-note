Feature "記事の作成・編集" do
  Scenario "アクセス制限" do
    Given "未ログイン状態のとき" do
      When "記事の作成・編集ページにアクセスすると、" do
        Then "ログインページにリダイレクトされる"
          .And "ログイン後に元のページへ戻る"
      end
    end

    Given "メールアドレスが未認証のとき" do
      When "記事の作成・編集ページにアクセスすると、" do
        Then "メールアドレスの認証が必要な旨と確認メールの再送ボタンが表示される"
      end
    end

    Given "他のユーザーの記事のとき" do
      When "保存しようとすると、" do
        Then "権限がない旨が表示される"
          .And "記事は更新されない"
      end
    end
  end

  Scenario "記事編集の基本操作" do
    Given None do
      When "タイトルを入力すると、" do
        Then "記事にタイトルが設定される"
      end

      When "タグを入力すると、" do
        Then "記事にタグが設定される"
      end

      When "本文に Markdown を入力すると、" do
        # PC では入力欄の横、モバイルではタブ切り替えでプレビューを表示する
        Then "本文が作成される"
          .And "プレビューにレンダリング結果がリアルタイムに表示される"
          .And "文字数のカウントが進む"
      end
    end
  end

  Scenario "コマンドメニュー" do
    Given None do
      When "本文で「/」を入力すると、" do
        Then "盤面エディタ、棋譜盤面、画像、見出し、リスト等の挿入メニューが表示される"
      end

      When "盤面エディタを選択すると、" do
        Then "盤面を編集するダイアログが表示される"
          .And "確定すると盤面が本文に挿入される"
      end

      When "棋譜盤面を選択すると、" do
        Then "登録済みの棋譜と表示する手数を選ぶダイアログが表示される"
          .And "確定すると棋譜プレイヤーが本文に挿入される"
      end
    end
  end

  Scenario "棋譜の登録" do
    Given None do
      When "棋譜を追加すると、" do
        Then "名前付きの棋譜が記事に登録され、棋譜一覧に表示される"
      end

      When "登録済みの棋譜を編集・削除すると、" do
        Then "棋譜一覧に反映される"
      end
    end
  end

  Scenario "画像の挿入" do
    Given "メールアドレス認証済みのとき" do
      When "コマンドメニュー・貼り付け・ドラッグ&ドロップで画像を挿入すると、" do
        Then "画像がアップロードされる"
          .And "本文に画像の記法が挿入される"
      end

      When "対応していない形式の画像を挿入しようとすると、" do
        Then "対応形式を案内するエラーメッセージが表示される"
          .And "アップロードされない"
      end
    end
  end

  Scenario "文字数の上限" do
    # 上限値の正は ArticleFormPage の maxCharacters
    Given "本文の文字数が上限を超えているとき" do
      When None do
        Then "文字数カウントが赤く表示される"
          .And "入力自体は出来る"
          .And "保存ボタンと投稿設定ボタンが押せない"
      end

      When "文字数を上限以下まで減らすと、" do
        Then "文字数カウントが通常の表示に戻る"
      end
    end
  end

  Scenario "必須項目" do
    Given "タイトルまたはタグが未設定のとき" do
      When None do
        Then "保存ボタンと投稿設定ボタンが押せない"
      end
    end
  end

  Scenario "ページを離れる" do
    Given "未保存の変更があるとき" do
      When "ページを離れようとすると、" do
        Then "ブラウザの離脱確認が表示される"
      end
    end
  end

  Scenario "新規記事の保存" do
    Given "タイトルとタグが設定されているとき" do
      When "保存ボタンを押すと、" do
        Then "記事が下書きとして保存される"
          .And "保存した記事のページへ遷移する"
      end
    end
  end

  Scenario "投稿" do
    Given "タイトルとタグが設定されているとき" do
      When "投稿設定ボタンを押すと、" do
        Then "「今すぐ公開」か「予約投稿」かを選ぶダイアログが表示される"
      end

      When "今すぐ公開を選んで公開すると、" do
        Then "記事が公開される"
          .And "記事のページへ遷移し、公開した旨の案内が表示される"
      end

      When "予約投稿で未来の日時を指定して公開すると、" do
        Then "記事は指定日時まで著者以外に公開されない"
          .And "指定日時を過ぎると公開記事として表示される"
      end

      When "予約投稿で過去の日時を指定すると、" do
        Then "未来の日時を選ぶよう促すエラーメッセージが表示される"
      end
    end
  end

  Scenario "既存記事の編集" do
    Given "既存記事の編集ページにアクセスしたとき" do
      When None do
        Then "記事のタイトル・タグ・棋譜・本文が入力済みの状態で表示される"
      end

      When "編集して保存ボタンを押すと、" do
        Then "公開状態はそのままで編集内容が保存される"
          .And "記事のページへ遷移する"
      end

      When "下書きを投稿設定から公開すると、" do
        Then "記事が公開される"
          .And "記事のページへ遷移し、公開した旨の案内が表示される"
      end
    end
  end

  Scenario "講座への記事の追加" do
    Given "講座の編集ページから記事を追加したとき" do
      When "記事を保存すると、" do
        Then "記事がその講座に属する記事として保存される"
      end
    end
  end
end
