Feature "トップページ" do
  Scenario "新着一覧の表示" do
    Given None do
      When "トップページにアクセスすると、" do
        # 講座に属する記事は講座側で表示するため、新着記事には単体記事のみを出す
        Then "「新着記事」と「新着講座」のタブが表示される"
          .And "新着記事タブには公開済みの単体記事が公開日時の新しい順に最大20件表示される"
          .And "各記事のタイトル、著者、タグ、いいね数、投稿日が表示される"
      end

      When "新着講座タブを選択すると、" do
        Then "公開済みの講座が作成日時の新しい順に表示される"
      end

      When "記事をクリックすると、" do
        Then "その記事のページに遷移する"
      end
    end
  end

  Scenario "トレンドタグ" do
    Given "画面幅が広いとき" do
      When None do
        Then "サイドバーに人気のタグが表示される"
      end

      When "タグをクリックすると、" do
        Then "そのタグのタグページに遷移する"
      end
    end
  end

  Scenario "ヘッダーのナビゲーション" do
    Given None do
      When None do
        Then "ホーム、検索、記事作成、講座作成へのリンクが表示される"
      end
    end

    Given "未ログイン状態のとき" do
      When None do
        Then "ログインボタンが表示される"
      end
    end

    Given "ログイン状態のとき" do
      When "ユーザーメニューを開くと、" do
        Then "マイページ、マイ記事一覧、マイ講座一覧、設定、ログアウトの項目が表示される"
      end
    end
  end
end
