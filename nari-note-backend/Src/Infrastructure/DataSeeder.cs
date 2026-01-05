using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Infrastructure;

public static class DataSeeder
{
    public static async Task SeedAsync(NariNoteDbContext context)
    {
        // すでにデータがある場合はスキップ
        if (await context.Users.AnyAsync())
        {
            return;
        }

        // ユーザー作成
        var users = new List<User>
        {
            new User
            {
                Name = "羽生九段ファン",
                Email = "habu-fan@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Bio = "将棋歴15年。居飛車党です。羽生先生の将棋を研究しています。",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new User
            {
                Name = "振り飛車党",
                Email = "furibisha@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Bio = "四間飛車一筋20年。藤井システムを愛用しています。",
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new User
            {
                Name = "詰将棋作家",
                Email = "tsume@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Bio = "詰将棋の創作と解答が趣味。毎日3問は解いています。",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new User
            {
                Name = "初心者将棋部",
                Email = "beginner@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Bio = "将棋を始めて半年。棒銀をマスター中です。",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // タグ作成
        var tags = new List<Tag>
        {
            new Tag { Name = "居飛車", CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new Tag { Name = "振り飛車", CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new Tag { Name = "詰将棋", CreatedAt = DateTime.UtcNow.AddDays(-28) },
            new Tag { Name = "戦法", CreatedAt = DateTime.UtcNow.AddDays(-25) },
            new Tag { Name = "初心者向け", CreatedAt = DateTime.UtcNow.AddDays(-25) },
            new Tag { Name = "棋譜解説", CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new Tag { Name = "プロ棋戦", CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new Tag { Name = "終盤", CreatedAt = DateTime.UtcNow.AddDays(-10) }
        };

        context.Tags.AddRange(tags);
        await context.SaveChangesAsync();

        // 記事作成
        var articles = new List<Article>
        {
            new Article
            {
                Title = "初心者のための棒銀戦法入門",
                Body = "棒銀は、初心者が最初に覚えるべき戦法の一つです。\n\n## 棒銀とは\n銀将を棒のようにまっすぐ進めて攻める戦法です。シンプルながら破壊力抜群！\n\n## 基本の駒組み\n1. ▲2六歩 △8四歩\n2. ▲2五歩 △8五歩\n3. ▲7八金 △3二金\n4. ▲3八銀 → ▲2七銀 → ▲2六銀\n\n## ポイント\n- 飛車先の歩を伸ばしてから銀を繰り出す\n- 相手の角頭（8七の地点）を狙う\n\nまずは棒銀をマスターして、将棋の基本を身につけましょう！",
                AuthorId = users[3].Id,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Article
            {
                Title = "四間飛車の基本と美濃囲い",
                Body = "四間飛車は振り飛車の基本形です。美濃囲いとセットで覚えましょう。\n\n## 四間飛車とは\n飛車を4筋（左から4番目）に振る戦法です。\n\n## 美濃囲いの組み方\n1. ▲6八玉\n2. ▲7八玉\n3. ▲5八金右\n4. ▲9八香（穴熊にする場合）\n\n## 四間飛車のメリット\n- 駒組みが覚えやすい\n- カウンター狙いの戦い方ができる\n- 美濃囲いが堅い\n\n振り飛車党を目指すなら、まず四間飛車から始めましょう！",
                AuthorId = users[1].Id,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new Article
            {
                Title = "5手詰めの解き方のコツ",
                Body = "詰将棋は終盤力を鍛える最高のトレーニングです。\n\n## 5手詰めを解くコツ\n\n### 1. 王手の種類を確認\n- 駒を打つ王手\n- 駒を動かす王手\n- 両王手\n\n### 2. 玉の逃げ道を塞ぐ\n詰ますためには、まず玉の退路を断つことが重要です。\n\n### 3. 捨て駒を恐れない\n派手な捨て駒から始まる詰みが多いです。\n\n## おすすめの詰将棋本\n- 「3手詰ハンドブック」\n- 「5手詰ハンドブック」\n\n毎日コツコツ解くことが上達の近道です！",
                AuthorId = users[2].Id,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Article
            {
                Title = "矢倉戦法の歴史と現代での評価",
                Body = "矢倉は「将棋の純文学」と呼ばれる格調高い戦法です。\n\n## 矢倉の歴史\n江戸時代から指されている伝統的な戦法で、長らく「相居飛車の王道」とされてきました。\n\n## 矢倉囲いの特徴\n- 上部に強い\n- 組み上がるまでに手数がかかる\n- 相矢倉では互いに攻め合いになりやすい\n\n## 現代での評価\n近年はソフト研究により急戦矢倉や雁木が増え、従来の矢倉は減少傾向にあります。\nしかし、矢倉の考え方を知ることは将棋の基礎力向上に役立ちます。\n\n一度は本格的な相矢倉を指してみてください！",
                AuthorId = users[0].Id,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Article
            {
                Title = "王位戦第5局の感想 - 藤井竜王の終盤力",
                Body = "先日行われた王位戦第5局を振り返ります。\n\n## 対局概要\n藤井竜王の先手番で、戦型は角換わり腰掛け銀に。\n\n## 見どころ\n中盤で挑戦者が優勢を築きましたが、藤井竜王の粘り強い指し回しが光りました。\n\n## 終盤のハイライト\n87手目の▲5五角が勝負を決めた一手。この手を境に形勢が逆転しました。\n読み筋を外された挑戦者は時間に追われ、最後は即詰みに討ち取られました。\n\n## まとめ\n改めて藤井竜王の終盤力の凄さを実感した一局でした。\n詰将棋で鍛えた読みの力が存分に発揮されていましたね。",
                AuthorId = users[0].Id,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        context.Articles.AddRange(articles);
        await context.SaveChangesAsync();

        // 記事とタグの関連付け
        var articleTags = new List<ArticleTag>
        {
            // 棒銀入門 -> 居飛車, 戦法, 初心者向け
            new ArticleTag { ArticleId = articles[0].Id, TagId = tags[0].Id, Article = articles[0], Tag = tags[0], CreatedAt = DateTime.UtcNow },
            new ArticleTag { ArticleId = articles[0].Id, TagId = tags[3].Id, Article = articles[0], Tag = tags[3], CreatedAt = DateTime.UtcNow },
            new ArticleTag { ArticleId = articles[0].Id, TagId = tags[4].Id, Article = articles[0], Tag = tags[4], CreatedAt = DateTime.UtcNow },
            // 四間飛車 -> 振り飛車, 戦法, 初心者向け
            new ArticleTag { ArticleId = articles[1].Id, TagId = tags[1].Id, Article = articles[1], Tag = tags[1], CreatedAt = DateTime.UtcNow },
            new ArticleTag { ArticleId = articles[1].Id, TagId = tags[3].Id, Article = articles[1], Tag = tags[3], CreatedAt = DateTime.UtcNow },
            new ArticleTag { ArticleId = articles[1].Id, TagId = tags[4].Id, Article = articles[1], Tag = tags[4], CreatedAt = DateTime.UtcNow },
            // 詰将棋 -> 詰将棋, 終盤, 初心者向け
            new ArticleTag { ArticleId = articles[2].Id, TagId = tags[2].Id, Article = articles[2], Tag = tags[2], CreatedAt = DateTime.UtcNow },
            new ArticleTag { ArticleId = articles[2].Id, TagId = tags[7].Id, Article = articles[2], Tag = tags[7], CreatedAt = DateTime.UtcNow },
            new ArticleTag { ArticleId = articles[2].Id, TagId = tags[4].Id, Article = articles[2], Tag = tags[4], CreatedAt = DateTime.UtcNow },
            // 矢倉 -> 居飛車, 戦法
            new ArticleTag { ArticleId = articles[3].Id, TagId = tags[0].Id, Article = articles[3], Tag = tags[0], CreatedAt = DateTime.UtcNow },
            new ArticleTag { ArticleId = articles[3].Id, TagId = tags[3].Id, Article = articles[3], Tag = tags[3], CreatedAt = DateTime.UtcNow },
            // 王位戦感想 -> プロ棋戦, 棋譜解説, 終盤
            new ArticleTag { ArticleId = articles[4].Id, TagId = tags[6].Id, Article = articles[4], Tag = tags[6], CreatedAt = DateTime.UtcNow },
            new ArticleTag { ArticleId = articles[4].Id, TagId = tags[5].Id, Article = articles[4], Tag = tags[5], CreatedAt = DateTime.UtcNow },
            new ArticleTag { ArticleId = articles[4].Id, TagId = tags[7].Id, Article = articles[4], Tag = tags[7], CreatedAt = DateTime.UtcNow }
        };

        context.ArticleTags.AddRange(articleTags);
        await context.SaveChangesAsync();

        // いいね作成
        var likes = new List<Like>
        {
            // React入門へのいいね
            new Like { UserId = users[1].Id, ArticleId = articles[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-4) },
            new Like { UserId = users[2].Id, ArticleId = articles[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new Like { UserId = users[3].Id, ArticleId = articles[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            // TypeScript完全ガイドへのいいね
            new Like { UserId = users[0].Id, ArticleId = articles[1].Id, CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new Like { UserId = users[2].Id, ArticleId = articles[1].Id, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new Like { UserId = users[3].Id, ArticleId = articles[1].Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            // Next.jsへのいいね
            new Like { UserId = users[0].Id, ArticleId = articles[2].Id, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new Like { UserId = users[1].Id, ArticleId = articles[2].Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            // データベース設計へのいいね
            new Like { UserId = users[0].Id, ArticleId = articles[3].Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Like { UserId = users[1].Id, ArticleId = articles[3].Id, CreatedAt = DateTime.UtcNow },
            // クリーンアーキテクチャへのいいね
            new Like { UserId = users[0].Id, ArticleId = articles[4].Id, CreatedAt = DateTime.UtcNow },
            new Like { UserId = users[2].Id, ArticleId = articles[4].Id, CreatedAt = DateTime.UtcNow },
            new Like { UserId = users[3].Id, ArticleId = articles[4].Id, CreatedAt = DateTime.UtcNow }
        };

        context.Likes.AddRange(likes);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ シードデータの投入が完了しました");
    }
}
