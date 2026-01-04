Feature "記事の基本機能" do
  # 注: 記事の基本機能は部分的に実装済みです。
  # - 記事取得API (GetArticle): 実装済み
  # - いいね機能API (ToggleLike): 実装済み
  # - 棋譜再生機能: 未実装
  Scenario "記事の表示" do
    Given None do
      When None do
        Then "記事のタイトルが表示される" 
          .And "記事の本文が表示される" 
          .And "記事のタグが表示される"
          .And "記事の作成日時が表示される"
          .And "記事の更新日時が表示される"
          .And "記事の著者がアイコンと名前で表示される"
      end
    end

    Given "記事に棋譜が含まれているとき" do
      When "再生ボタンを押すと、" do
        Then "棋譜の再生が始まる"
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
        Then "そのタグのタグタイムラインページに遷移する"
      end
    end
  end

  Scenario "いいね機能" do
    Given "未ログイン状態のとき" do
      When "いいねボタンを押すと、" do
        Then "ログインページに遷移するモーダルが表示される"
      end
    end

    Given "ログイン状態のとき" do
      Given "記事にいいねしていないとき" do
        When "いいねボタンを押すと、" do
          Then "記事にいいねが追加される"
            .And "いいねボタンの色が変わる"
            .And "いいね数が1増える"
        end
      end

      Given "記事にいいねしているとき" do
        When "いいねボタンを押すと、" do
          Then "記事のいいねが取り消される"
            .And "いいねボタンの色が元に戻る"
            .And "いいね数が1減る"
        end
      end
    end
  end
end
