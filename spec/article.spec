Feature "記事の閲覧" do
  Scenario "記事の表示" do
    Given None do
      When "記事ページにアクセスすると、" do
        Then "記事のタイトルが表示される"
          .And "記事の本文が Markdown としてレンダリングされて表示される"
          .And "記事のタグが表示される"
          .And "記事の著者がアイコンと名前で表示される"
          .And "記事の作成日が表示される"
          .And "いいね数が表示される"
          .And "X への共有ボタンが表示される"
          .And "コメント一覧とコメント入力欄が表示される"
      end
    end

    Given "本文に盤面が含まれているとき" do
      When None do
        Then "将棋の盤面が表示される"
      end
    end

    Given "本文に棋譜が含まれているとき" do
      When None do
        Then "棋譜プレイヤーが指定の手数の局面で表示される"
      end

      When "棋譜プレイヤーで手を進めると、" do
        Then "局面が進む"
      end
    end

    Given "記事が講座に属しているとき" do
      When None do
        Then "講座へのリンクが表示される"
      end
    end

    Given "自分の記事のとき" do
      When None do
        Then "編集リンクが表示される"
      end
    end
  end

  Scenario "未公開記事の閲覧" do
    Given "記事が下書きまたは予約投稿で公開日時前のとき" do
      When "著者本人がアクセスすると、" do
        Then "記事が表示される"
      end

      When "著者以外がアクセスすると、" do
        Then "記事が見つからない旨が表示される"
      end
    end
  end

  Scenario "著者プロフィールへのアクセス" do
    Given None do
      When "著者のアイコンか名前をクリックすると、" do
        Then "著者のプロフィールページに遷移する"
      end
    end
  end

  Scenario "タグのクリック" do
    Given None do
      When "記事のタグをクリックすると、" do
        Then "そのタグのタグページに遷移する"
      end
    end
  end

  Scenario "いいね" do
    Given "未ログイン状態のとき" do
      When "いいねボタンを押すと、" do
        Then "ログインを促すモーダルが表示される"
      end
    end

    Given "メールアドレスが未認証のとき" do
      When "いいねボタンを押すと、" do
        Then "メールアドレスの認証を促すモーダルが表示される"
      end
    end

    Given "メールアドレス認証済みでログインしているとき" do
      Given "記事にいいねしていないとき" do
        When "いいねボタンを押すと、" do
          Then "記事にいいねが追加される"
            .And "いいねボタンの表示が変わる"
            .And "いいね数が1増える"
        end
      end

      Given "記事にいいねしているとき" do
        When "いいねボタンを押すと、" do
          Then "記事のいいねが取り消される"
            .And "いいねボタンの表示が元に戻る"
            .And "いいね数が1減る"
        end
      end
    end
  end

  Scenario "コメント" do
    Given None do
      When None do
        Then "コメントが投稿日時の古い順に、投稿者のアイコン・名前と共に表示される"
      end
    end

    Given "メールアドレス認証済みでログインしているとき" do
      When "コメントを入力して投稿すると、" do
        Then "コメントが投稿される"
          .And "コメント一覧に追加される"
      end

      When "空のコメントを投稿しようとすると、" do
        Then "入力を促すエラーメッセージが表示される"
          .And "投稿されない"
      end
    end

    Given "未ログイン状態のとき" do
      When "コメントを投稿しようとすると、" do
        Then "ログインを促すモーダルが表示される"
      end
    end

    Given "メールアドレスが未認証のとき" do
      When "コメントを投稿しようとすると、" do
        Then "メールアドレスの認証を促すモーダルが表示される"
      end
    end
  end

  Scenario "公開直後の案内" do
    Given "記事を公開して記事ページに遷移したとき" do
      When None do
        Then "公開した旨と X での共有を促す案内が表示される"
      end

      When "案内を閉じると、" do
        Then "案内が非表示になる"
      end
    end
  end
end
