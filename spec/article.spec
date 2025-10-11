Feature "記事の基本機能" do
  Scenario "記事の表示" do
    Given None do
      When None do
        Then "記事のタイトルが表示される" 
          .And "記事の本文が表示される" 
          .And "記事のタグが表示される"
          .And "記事の作成日時が表示される"
          .And "記事の更新日時が表示される"
          .And "記事の著者が表示される"
      end
    end
  end
end
