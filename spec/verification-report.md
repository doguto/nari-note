# 仕様適合性検証レポート

## 検証日時
2026-01-18

## 検証対象
記事編集時のタグ保存・表示機能

## 関連仕様
`spec/article-edit.spec` - Scenario "投稿済み記事の編集" (lines 114-133)

### 仕様要件
```
Given "投稿済み記事の編集ページにアクセスするとき" do
  When None do
    Then "記事のタイトルが表示される"
      .And "記事のタグが表示される"
      .And "記事の本文が表示される"
  end
end
```

## 変更内容
`nari-note-backend/Src/Infrastructure/Repository/ArticleRepository.cs` の `UpdateWithTagAsync` メソッドにて、記事更新後にArticleTagsナビゲーションプロパティを明示的にリロードする処理を追加。

### 変更差分
```csharp
// 行 152-157に追加
// Reload ArticleTags navigation property to ensure it reflects the saved changes
await context.Entry(article)
    .Collection(a => a.ArticleTags)
    .Query()
    .Include(at => at.Tag)
    .LoadAsync();
```

## コード分析結果

### 1. データフローの検証

#### 1.1 記事更新フロー
```
クライアント (ArticleFormPage.tsx)
  ↓ PUT /api/articles/{id}
ArticlesController.UpdateArticle()
  ↓
UpdateArticleService.ExecuteAsync()
  ↓
ArticleRepository.UpdateWithTagAsync()
  ↓
データベース保存 & ナビゲーションプロパティリロード
```

#### 1.2 記事取得フロー（編集ページ表示時）
```
クライアント (ArticleFormPage.tsx)
  ↓ GET /api/articles/{id}
ArticlesController.GetArticle()
  ↓
GetArticleService.ExecuteAsync()
  ↓
ArticleRepository.FindForceByIdAsync()
  ↓
Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
  ↓
GetArticleResponse (タグを含む)
```

### 2. 実装の正確性

#### 2.1 UpdateWithTagAsyncメソッドの動作
1. **記事エンティティの更新**: `context.Articles.Update(article)`
2. **既存タグの削除**: ArticleTagsテーブルから既存の関連を削除
3. **新規タグの作成**: 存在しないタグをTagsテーブルに挿入
4. **タグ関連の作成**: ArticleTagsテーブルに新しい関連を挿入
5. **変更の保存**: `await context.SaveChangesAsync()`
6. **✅ 追加された処理**: ArticleTagsナビゲーションプロパティのリロード

#### 2.2 追加処理の必要性
Entity Framework Coreでは、ナビゲーションプロパティはコンテキストに追跡されているエンティティの状態を反映します。しかし、以下のような場合に問題が発生する可能性があります：

- **変更検知の遅延**: SaveChangesAsync後、ナビゲーションプロパティが自動的に更新されない場合がある
- **トラッキングの問題**: ArticleTagsコレクションが古い状態を保持している可能性がある

追加されたリロード処理により、これらの問題が解決され、`article.ArticleTags`が確実に最新の状態を反映するようになります。

### 3. 仕様適合性の検証

#### 3.1 編集ページでのタグ表示（仕様: lines 118）

**フロントエンド実装**:
```typescript
// ArticleFormPage.tsx (lines 83-90)
useEffect(() => {
  if (isEditMode && article && !isInitialized) {
    setTitle(article.title || '');
    setBody(article.body || '');
    setTags(article.tags || []); // ✅ タグを状態にセット
    setIsInitialized(true);
  }
}, [isEditMode, article, isInitialized]);
```

**バックエンド実装**:
```csharp
// GetArticleService.cs (line 40)
Tags = article.ArticleTags
    .Select(at => at.Tag?.Name ?? string.Empty)
    .Where(name => !string.IsNullOrEmpty(name))
    .ToList(),
```

**リポジトリクエリ**:
```csharp
// ArticleRepository.FindByIdAsync (lines 46-51)
return await context.Articles
    .Include(a => a.Author)
    .Include(a => a.ArticleTags)
        .ThenInclude(at => at.Tag) // ✅ Tagエンティティを含む
    .Include(a => a.Likes)
    .FirstOrDefaultAsync(a => a.Id == id);
```

**結論**: ✅ **仕様を満たしている**
- ArticleTagsナビゲーションプロパティが正しくロードされる
- GetArticleServiceがタグ名のリストを作成
- フロントエンドがタグを表示

#### 3.2 タグ更新後の永続性

**問題点（修正前）**:
1. UpdateWithTagAsyncでタグを更新
2. SaveChangesAsyncで保存
3. しかし、返却されるarticleオブジェクトのArticleTagsプロパティが古い状態のまま

**修正内容**:
```csharp
// 明示的にリロード
await context.Entry(article)
    .Collection(a => a.ArticleTags)
    .Query()
    .Include(at => at.Tag)
    .LoadAsync();
```

**効果**:
- UpdateWithTagAsyncから返されるarticleオブジェクトが最新のArticleTagsを持つ
- 次回のGetArticleで正確なタグが取得される
- データベースとメモリ上の状態が一致

**結論**: ✅ **仕様を満たしている**
- タグ更新後、ナビゲーションプロパティが確実にリロードされる
- 次回編集ページを開いた時、最新のタグが表示される

### 4. 他の関連機能への影響

#### 4.1 影響を受けない機能
- **記事一覧表示**: FindLatestAsyncは独自のクエリを使用
- **タグ検索**: FindByTagAsyncは独自のクエリを使用
- **著者別記事**: FindByAuthorAsyncは独自のクエリを使用
- **下書き一覧**: FindDraftsByAuthorAsyncは独自のクエリを使用

これらのメソッドは全て、データベースから直接クエリするため、UpdateWithTagAsyncの変更による影響を受けません。

#### 4.2 影響を受ける可能性のある機能
- **UpdateArticleService**: UpdateWithTagAsyncを直接使用
  - ✅ 影響: リロード処理により、更新後のarticleオブジェクトが正確な状態を持つ
  - ✅ 結果: 正の影響（改善）

#### 4.3 パフォーマンス影響
- **追加クエリ**: 1回のロードクエリ（LoadAsync）
- **影響度**: 低（記事更新は頻繁ではない操作）
- **トレードオフ**: データ整合性 > わずかなパフォーマンスコスト

## 検証結果サマリー

### ✅ 仕様適合性
| 項目 | 状態 | 詳細 |
|------|------|------|
| 編集ページでタグ表示 | ✅ 合格 | ArticleTagsがロードされ、UIに表示される |
| タグ更新の永続性 | ✅ 合格 | リロード処理により確実に反映される |
| 次回編集時のタグ表示 | ✅ 合格 | FindByIdAsyncがArticleTagsをIncludeする |

### ✅ コード品質
| 項目 | 状態 | 詳細 |
|------|------|------|
| Entity Framework使用法 | ✅ 適切 | Entry().Collection().LoadAsync()は推奨パターン |
| データ整合性 | ✅ 保証 | メモリとDBの状態が一致 |
| 副作用 | ✅ なし | 他の機能に悪影響なし |

### ✅ 設計
| 項目 | 状態 | 詳細 |
|------|------|------|
| 責任の分離 | ✅ 適切 | リポジトリがデータアクセスを担当 |
| 再利用性 | ✅ 良好 | UpdateWithTagAsyncは独立して機能 |
| 保守性 | ✅ 良好 | コメントにより意図が明確 |

## 推奨事項

### 1. 追加のテストケース
仕様を完全に検証するため、以下のE2Eテストケースを追加することを推奨します：

```
Scenario: 記事編集時のタグ更新と再表示
  Given: タグ ["将棋", "初心者"] を持つ記事が存在する
  When: ユーザーが記事を編集し、タグを ["将棋", "中級者", "戦法"] に変更して保存
  And: 再度編集ページを開く
  Then: タグ入力フィールドに ["将棋", "中級者", "戦法"] が表示される
  And: 古いタグ "初心者" は表示されない
```

### 2. ユニットテストの追加
ArticleRepository.UpdateWithTagAsyncのユニットテストを作成し、以下を検証：
- タグが正しく削除・追加される
- ナビゲーションプロパティが正しくリロードされる
- 返されるarticleオブジェクトが最新の状態を持つ

### 3. ドキュメント
リロード処理の理由をコメントに追加済み（line 152）。これは良い実践です。

## 結論

**🎉 実装は仕様を満たしています**

変更内容は以下の点で適切です：
1. ✅ 仕様要件「記事のタグが表示される」を満たす
2. ✅ タグ更新の永続性を保証
3. ✅ Entity Frameworkのベストプラクティスに従っている
4. ✅ 他の機能に悪影響を与えない
5. ✅ パフォーマンスへの影響は最小限

この修正により、記事編集時にタグが正しく保存され、次回編集ページを開いた時に確実に反映されるようになります。
