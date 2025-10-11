Feature "記事の編集機能" do
  Scenario "記事編集の基本操作" do
    Given None do
      When "タイトル用のテキストボックスに入力すると、" do
        Then "記事にタイトルが設定される"
      end

      When "タグ用のテキストボックスに入力すると、" do
        Then "記事にタグが設定される"
      end

      When "本文に入力すると、" do
        Then "本文が作成される" .And "文字数のカウントが進む"
      end
    end
  end

  Scenario "本文への盤面の挿入" do
    Given None do
      When "本文テキストボックスで「/」を入力すると、" do
        Then "挿入メニューが表示される"
      end

      When "挿入メニューから盤面を選択すると、" do
        Then "盤面フォーマットが本文に挿入される"
      end

      When "挿入メニューから棋譜を選択すると、" do
        Then "棋譜フォーマットが本文に挿入される"
      end
    end
  end

  Scenario "文字数の上限に到達" do
    # 現状5万文字を想定
    # TEXT型の上限文字数が65535文字であるため
    Given "本文の文字数が上限に達しているとき" do
      When "本文に文字を入力すると、" do
        Then "文字数カウントが赤くなる"
          .And "カウントが進む"
          .And "入力自体は出来る"
      end

      When "文字数を上限以下まで減らすと、" do
        Then "文字数カウントが通常の色に戻る"
      end

      When "下書き保存ボタンを押すと、" do
        Then "文字数超過の旨のアラートが表示される"
          .And "下書き保存はされない"
      end

      When "投稿ボタンを押すと、" do
        Then "文字数超過の旨のアラートが表示される"
          .And "投稿はされない"
      end
    end
  end

  Scenario "プレビューの表示" do
    Given None do
      When "プレビューボタンを押すと、" do
        # 理想は画面を分割するなどしてリアルタイムにプレビューを確認できることだが、
        # 実装コストの関係で一旦はボタン押下でプレビュー画面に遷移する形にする
        Then "記事のプレビューが表示される"
      end

      When "本文に盤面のフォーマットが含まれていると、" do
        Then "設定した将棋の盤面が正しくレンダリングされる"
      end

      When "本文に棋譜のフォーマットが含まれていると、" do
        Then "設定した棋譜の最初の盤面が正しくレンダリングされる"
          .And "棋譜の再生コントロールが表示される"
      end
    end
  end

  Scenario "ページを離れる" do
    Given None do
      When "ページを離れようとすると、" do
        Then "編集内容が失われる旨の確認アラートが出る"
      end
    end
  end

  Scenario "編集内容の自動保存" do
    Given None do
      When "一定時間が経過すると、" do
        Then "編集内容が自動保存される"
      end
    end
  end

  Scenario "下書きの保存" do
    Given "タイトルが設定されているとき".And "タグが設定されているとき" do
      When "下書き保存ボタンを押すと、" do
        Then "記事が下書きとして保存される"
          .And "下書き一覧ページへと遷移する"
      end
    end
  end

  Scenario "投稿" do
    Given "タイトルが設定されているとき".And "タグが設定されているとき" do
      When "投稿ボタンを押すと、" do
        # 公開範囲の設定等は無し
        Then "記事が公開される"
          .And "公開した記事のページへと遷移する"
      end
    end
  end

  Scenario "サインインの要求" do
    Given "サインインしていないとき" do
      When "記事の編集を試みると、" do
        Then "サインインを要求される"
          .And "サインインページにリダイレクトされる"
      end
    end
  end
end
