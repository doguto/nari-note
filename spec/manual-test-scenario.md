# 手動テストシナリオ: 記事編集時のタグ保存・表示

## 前提条件
- ユーザーがログインしている
- テスト用の記事が存在する

## テストケース1: 新規記事作成とタグ設定

### 手順
1. `/articles/new` にアクセス
2. タイトルに「テスト記事」を入力
3. タグに「将棋」「初心者」を追加
4. 本文に「テスト本文」を入力
5. 「投稿」ボタンをクリック
6. 記事詳細ページで記事IDをメモ

### 期待結果
- ✅ 記事が正常に作成される
- ✅ 記事詳細ページにタグ「将棋」「初心者」が表示される

---

## テストケース2: 記事編集ページでのタグ表示

### 手順
1. テストケース1で作成した記事の編集ページにアクセス (`/articles/{id}/edit`)
2. タグ入力フィールドを確認

### 期待結果
- ✅ タイトル「テスト記事」が表示される
- ✅ タグ「将棋」「初心者」が表示される
- ✅ 本文「テスト本文」が表示される

**仕様対応**: `spec/article-edit.spec` lines 117-119

---

## テストケース3: タグの更新

### 手順
1. 編集ページで既存のタグ「初心者」を削除
2. 新しいタグ「中級者」「戦法」を追加
3. 「投稿」ボタンをクリック（更新）
4. 記事詳細ページを確認

### 期待結果
- ✅ 記事が正常に更新される
- ✅ 記事詳細ページにタグ「将棋」「中級者」「戦法」が表示される
- ✅ 削除したタグ「初心者」は表示されない

---

## テストケース4: 更新後の再編集でタグが反映される（重要）

### 手順
1. テストケース3で更新した記事の編集ページに再度アクセス (`/articles/{id}/edit`)
2. タグ入力フィールドを確認

### 期待結果
- ✅ タグ「将棋」「中級者」「戦法」が表示される
- ✅ 削除したタグ「初心者」は表示されない

**仕様対応**: `spec/article-edit.spec` lines 118
**修正対応**: この動作を保証するための修正

---

## テストケース5: タグを空にする

### 手順
1. 編集ページで全てのタグを削除
2. 「投稿」ボタンをクリック

### 期待結果
- ❌ バリデーションエラーが表示される
- ❌ 「少なくとも1つのタグを追加してください」というメッセージ

**フロントエンド実装**: `ArticleFormPage.tsx` lines 142-144

---

## テストケース6: 複数回の編集とタグ変更

### 手順
1. 編集ページでタグを「A」「B」に変更して保存
2. 再度編集ページを開き、タグ「A」「B」が表示されることを確認
3. タグを「C」「D」「E」に変更して保存
4. 再度編集ページを開き、タグ「C」「D」「E」が表示されることを確認

### 期待結果
- ✅ 各編集後、次回編集ページを開いた時に最新のタグが正しく表示される
- ✅ 前回のタグは表示されない

---

## API レベルの検証

### GET /api/articles/{id} レスポンス確認

```bash
# 記事取得
curl -X GET "http://localhost:5000/api/articles/{id}" \
  -H "Authorization: Bearer {token}"
```

**期待JSONレスポンス**:
```json
{
  "id": 123,
  "title": "テスト記事",
  "body": "テスト本文",
  "tags": ["将棋", "中級者", "戦法"],
  "authorId": 456,
  "authorName": "テストユーザー",
  "isPublished": true,
  ...
}
```

### PUT /api/articles/{id} リクエスト&レスポンス確認

```bash
# 記事更新
curl -X PUT "http://localhost:5000/api/articles/{id}" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "title": "更新後のタイトル",
    "body": "更新後の本文",
    "tags": ["新タグ1", "新タグ2"],
    "isPublished": true
  }'
```

**期待結果**:
```json
{
  "id": 123,
  "updatedAt": "2026-01-18T15:30:00Z"
}
```

### データベース直接確認

```sql
-- 記事のタグを確認
SELECT 
    a.Id as ArticleId,
    a.Title,
    t.Name as TagName
FROM 
    Articles a
    INNER JOIN ArticleTags at ON a.Id = at.ArticleId
    INNER JOIN Tags t ON at.TagId = t.Id
WHERE 
    a.Id = {article_id};
```

**期待結果**: 最新のタグが正しくArticleTagsテーブルに保存されている

---

## 自動テストコード案

```csharp
[Fact]
public async Task UpdateWithTagAsync_ShouldReloadArticleTagsNavigationProperty()
{
    // Arrange
    var article = new Article { /* ... */ };
    var initialTags = new List<string> { "Tag1", "Tag2" };
    await repository.CreateAsync(article);
    await repository.UpdateWithTagAsync(article, initialTags);
    
    // Act
    var newTags = new List<string> { "Tag3", "Tag4", "Tag5" };
    var updatedArticle = await repository.UpdateWithTagAsync(article, newTags);
    
    // Assert
    Assert.Equal(3, updatedArticle.ArticleTags.Count);
    Assert.Contains(updatedArticle.ArticleTags, at => at.Tag.Name == "Tag3");
    Assert.Contains(updatedArticle.ArticleTags, at => at.Tag.Name == "Tag4");
    Assert.Contains(updatedArticle.ArticleTags, at => at.Tag.Name == "Tag5");
    Assert.DoesNotContain(updatedArticle.ArticleTags, at => at.Tag.Name == "Tag1");
    Assert.DoesNotContain(updatedArticle.ArticleTags, at => at.Tag.Name == "Tag2");
}
```

---

## 確認済みポイント

### コードレビュー
- ✅ UpdateWithTagAsyncメソッドにリロード処理が追加されている
- ✅ Entity Frameworkの適切なAPIを使用している
- ✅ Include(at => at.Tag)でTagエンティティもロードされる
- ✅ コメントにより意図が明確に記載されている

### データフロー
- ✅ UpdateArticleService → ArticleRepository.UpdateWithTagAsync
- ✅ GetArticleService → ArticleRepository.FindByIdAsync (ArticleTagsをInclude)
- ✅ フロントエンド: ArticleFormPage がタグを状態管理

### 仕様適合性
- ✅ spec/article-edit.spec line 118: "記事のタグが表示される"
- ✅ タグ更新の永続性が保証される
- ✅ 次回編集ページアクセス時に最新タグが表示される

---

## 結論

**実装は仕様要件を満たしています。**

修正により、以下が保証されます：
1. 記事更新時にArticleTagsナビゲーションプロパティが確実に最新状態になる
2. UpdateWithTagAsyncから返されるarticleオブジェクトが正確なタグ情報を持つ
3. 次回GetArticleで取得する際、最新のタグが取得される
4. 編集ページを再度開いた時、正しいタグが表示される
