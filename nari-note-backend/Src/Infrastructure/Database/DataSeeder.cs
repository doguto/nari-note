using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public static class DataSeeder
{
    public static async Task SeedAsync(NariNoteDbContext context)
    {
        // すでにデータがある場合はスキップ
        if (await context.Users.AnyAsync()) return;

        // ユーザー作成
        // ⚠️ 注意: 開発環境用のシードデータです。本番環境では使用しないでください。
        // パスワード: password123 (セキュリティ上、本番環境ではより強力なパスワードを使用してください)
        var users = new List<User>
        {
            new()
            {
                Id = UserId.From(1),
                Name = "羽生九段ファン",
                Email = "habu-fan@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Bio = "将棋歴15年。居飛車党です。羽生先生の将棋を研究しています。",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Id = UserId.From(2),
                Name = "振り飛車党",
                Email = "furibisha@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Bio = "四間飛車一筋20年。藤井システムを愛用しています。",
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new()
            {
                Id = UserId.From(3),
                Name = "詰将棋作家",
                Email = "tsume@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Bio = "詰将棋の創作と解答が趣味。毎日3問は解いています。",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new()
            {
                Id = UserId.From(4),
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
            new() { Id = TagId.From(1), Name = "居飛車", CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new() { Id = TagId.From(2), Name = "振り飛車", CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new() { Id = TagId.From(3), Name = "詰将棋", CreatedAt = DateTime.UtcNow.AddDays(-28) },
            new() { Id = TagId.From(4), Name = "戦法", CreatedAt = DateTime.UtcNow.AddDays(-25) },
            new() { Id = TagId.From(5), Name = "初心者向け", CreatedAt = DateTime.UtcNow.AddDays(-25) },
            new() { Id = TagId.From(6), Name = "棋譜解説", CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new() { Id = TagId.From(7), Name = "プロ棋戦", CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new() { Id = TagId.From(8), Name = "終盤", CreatedAt = DateTime.UtcNow.AddDays(-10) }
        };

        context.Tags.AddRange(tags);
        await context.SaveChangesAsync();

        // 記事作成
        var articles = new List<Article>
        {
            new()
            {
                Id = ArticleId.From(1),
                Title = "初心者のための棒銀戦法入門",
                Body =
                    "棒銀は、初心者が最初に覚えるべき戦法の一つです。\n\n## 棒銀とは\n銀将を棒のようにまっすぐ進めて攻める戦法です。シンプルながら破壊力抜群！\n\n## 基本の駒組み\n1. ▲２六歩 △８四歩\n2. ▲２五歩 △８五歩\n3. ▲７八金 △３二金\n4. ▲３八銀 → ▲２七銀 → ▲２六銀\n\n## ポイント\n- 飛車先の歩を伸ばしてから銀を繰り出す\n- 相手の角頭（８七の地点）を狙う\n\nまずは棒銀をマスターして、将棋の基本を身につけましょう！",
                AuthorId = users[3].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = ArticleId.From(2),
                Title = "四間飛車の基本と美濃囲い",
                Body =
                    "四間飛車は振り飛車の基本形です。美濃囲いとセットで覚えましょう。\n\n## 四間飛車とは\n飛車を４筋（左から４番目）に振る戦法です。\n\n## 美濃囲いの組み方\n1. ▲６八玉\n2. ▲７八玉\n3. ▲５八金右\n4. ▲９八香（穴熊にする場合）\n\n## 四間飛車のメリット\n- 駒組みが覚えやすい\n- カウンター狙いの戦い方ができる\n- 美濃囲いが堅い\n\n振り飛車党を目指すなら、まず四間飛車から始めましょう！",
                AuthorId = users[1].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-4),
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new()
            {
                Id = ArticleId.From(3),
                Title = "５手詰めの解き方のコツ",
                Body =
                    "詰将棋は終盤力を鍛える最高のトレーニングです。\n\n## ５手詰めを解くコツ\n\n### 1. 王手の種類を確認\n- 駒を打つ王手\n- 駒を動かす王手\n- 両王手\n\n### 2. 玉の逃げ道を塞ぐ\n詰ますためには、まず玉の退路を断つことが重要です。\n\n### 3. 捨て駒を恐れない\n派手な捨て駒から始まる詰みが多いです。\n\n## おすすめの詰将棋本\n- 「３手詰ハンドブック」\n- 「５手詰ハンドブック」\n\n毎日コツコツ解くことが上達の近道です！",
                AuthorId = users[2].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = ArticleId.From(4),
                Title = "矢倉戦法の歴史と現代での評価",
                Body =
                    "矢倉は「将棋の純文学」と呼ばれる格調高い戦法です。\n\n## 矢倉の歴史\n江戸時代から指されている伝統的な戦法で、長らく「相居飛車の王道」とされてきました。\n\n## 矢倉囲いの特徴\n- 上部に強い\n- 組み上がるまでに手数がかかる\n- 相矢倉では互いに攻め合いになりやすい\n\n## 現代での評価\n近年はソフト研究により急戦矢倉や雁木が増え、従来の矢倉は減少傾向にあります。\nしかし、矢倉の考え方を知ることは将棋の基礎力向上に役立ちます。\n\n一度は本格的な相矢倉を指してみてください！",
                AuthorId = users[0].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = ArticleId.From(5),
                Title = "王位戦第5局の感想 - 藤井竜王の終盤力",
                Body =
                    "先日行われた王位戦第5局を振り返ります。\n\n## 対局概要\n藤井竜王の先手番で、戦型は角換わり腰掛け銀に。\n\n## 見どころ\n中盤で挑戦者が優勢を築きましたが、藤井竜王の粘り強い指し回しが光りました。\n\n## 終盤のハイライト\n87手目の▲5五角が勝負を決めた一手。この手を境に形勢が逆転しました。\n読み筋を外された挑戦者は時間に追われ、最後は即詰みに討ち取られました。\n\n## まとめ\n改めて藤井竜王の終盤力の凄さを実感した一局でした。\n詰将棋で鍛えた読みの力が存分に発揮されていましたね。",
                AuthorId = users[0].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = ArticleId.From(6),
                Title = "角換わりの最新定跡",
                Body =
                    "角換わりは相居飛車の代表的な戦型です。\n\n## 角換わりとは\n序盤で角を交換し、その後の展開を競う戦型です。\n\n## 主な形\n- 腰掛け銀\n- 早繰り銀\n- 棒銀\n\n## 最近のトレンド\n近年はAI研究により、従来の定跡が大きく変化しています。\n特に先手の▲4五桂速攻が注目されています。\n\n角換わりを極めて、相居飛車を得意戦法にしましょう!",
                AuthorId = users[0].Id,
                PublishedAt = DateTime.UtcNow.AddHours(-12),
                CreatedAt = DateTime.UtcNow.AddHours(-12),
                UpdatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new()
            {
                Id = ArticleId.From(7),
                Title = "三間飛車で勝つための心得",
                Body =
                    "三間飛車は振り飛車の中でも攻撃的な戦法です。\n\n## 三間飛車の特徴\n飛車を3筋に振り、積極的に仕掛けていく戦法です。\n\n## 基本的な攻め筋\n1. 石田流に組む\n2. ▲3五歩から仕掛ける\n3. 角交換を狙う\n\n## 囲いの選択\n- 美濃囲い(基本形)\n- 穴熊(じっくり戦う場合)\n- 金無双(速攻の場合)\n\n## まとめ\n三間飛車は攻めの将棋を楽しみたい方におすすめです!",
                AuthorId = users[1].Id,
                PublishedAt = DateTime.UtcNow.AddHours(-6),
                CreatedAt = DateTime.UtcNow.AddHours(-6),
                UpdatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new()
            {
                Id = ArticleId.From(8),
                Title = "初心者が覚えるべき3つの格言",
                Body =
                    "将棋には先人の知恵が詰まった格言があります。\n\n## 1. 王様は下段に落とせ\n詰ますためには、相手玉を下段に追い込むことが重要です。\n\n## 2. 玉の早逃げ八手の得\n危険を感じたら、早めに玉を逃がすことが大切です。\n攻め合いで後手を引かないようにしましょう。\n\n## 3. 桂馬は端に追え\n桂馬は前にしか進めないため、端に追い詰めると活用しづらくなります。\n\n## おわりに\n格言を意識することで、考え方の幅が広がります。\nまずはこの3つを実戦で使ってみてください!",
                AuthorId = users[3].Id,
                PublishedAt = DateTime.UtcNow.AddHours(-3),
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                UpdatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new()
            {
                Id = ArticleId.From(9),
                Title = "実戦詰将棋:3手詰めにチャレンジ",
                Body =
                    "実戦でよく現れる3手詰めを解いてみましょう。\n\n## 問題\n持ち駒: 金\n\n```\n  9   8   7   6   5   4   3   2   1\n┌───────────────────┐\n│ v玉 ・ ・ │一\n│ 銀 金 ・ │二\n│ ・ ・ ・ │三\n└───────────────────┘\n```\n\n## ヒント\n持ち駒の金をどこに打つか考えてみましょう。\n玉の逃げ道を塞ぐことがポイントです。\n\n## 解答\n▲1一金 △9二玉 ▲8二金まで\n\n基本的な形なので、実戦で見逃さないようにしましょう!",
                AuthorId = users[2].Id,
                PublishedAt = DateTime.UtcNow.AddHours(-2),
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = ArticleId.From(10),
                Title = "将棋ソフトを使った棋力向上法",
                Body =
                    "将棋ソフトを活用して効率的に上達する方法を紹介します。\n\n## おすすめの将棋ソフト\n- 将棋ぶらうざQ(無料)\n- 技巧(オープンソース)\n- elmo(強豪ソフト)\n\n## 活用方法\n\n### 1. 棋譜解析\n自分の対局をソフトで検討し、悪手を見つけます。\nどこで形勢が傾いたか確認しましょう。\n\n### 2. 定跡の確認\n最新の定跡をソフトで調べることができます。\n\n### 3. 詰みの確認\n終盤で詰みを逃した場面を検証できます。\n\n## 注意点\nソフトに頼りすぎず、自分で考える力を大切にしましょう。\nソフトの評価値だけでなく、なぜその手が良いのか理解することが重要です。",
                AuthorId = users[0].Id,
                PublishedAt = DateTime.UtcNow.AddHours(-1),
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        context.Articles.AddRange(articles);
        await context.SaveChangesAsync();

        // 記事とタグの関連付け
        var articleTags = new List<ArticleTag>
        {
            // 棒銀入門 -> 居飛車, 戦法, 初心者向け
            new()
            {
                Id = ArticleTagId.From(1), ArticleId = articles[0].Id, TagId = tags[0].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ArticleTagId.From(2), ArticleId = articles[0].Id, TagId = tags[3].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ArticleTagId.From(3), ArticleId = articles[0].Id, TagId = tags[4].Id, CreatedAt = DateTime.UtcNow
            },
            // 四間飛車 -> 振り飛車, 戦法, 初心者向け
            new()
            {
                Id = ArticleTagId.From(4), ArticleId = articles[1].Id, TagId = tags[1].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ArticleTagId.From(5), ArticleId = articles[1].Id, TagId = tags[3].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ArticleTagId.From(6), ArticleId = articles[1].Id, TagId = tags[4].Id, CreatedAt = DateTime.UtcNow
            },
            // 詰将棋 -> 詰将棋, 終盤, 初心者向け
            new()
            {
                Id = ArticleTagId.From(7), ArticleId = articles[2].Id, TagId = tags[2].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ArticleTagId.From(8), ArticleId = articles[2].Id, TagId = tags[7].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ArticleTagId.From(9), ArticleId = articles[2].Id, TagId = tags[4].Id, CreatedAt = DateTime.UtcNow
            },
            // 矢倉 -> 居飛車, 戦法
            new()
            {
                Id = ArticleTagId.From(10), ArticleId = articles[3].Id, TagId = tags[0].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ArticleTagId.From(11), ArticleId = articles[3].Id, TagId = tags[3].Id, CreatedAt = DateTime.UtcNow
            },
            // 王位戦感想 -> プロ棋戦, 棋譜解説, 終盤
            new()
            {
                Id = ArticleTagId.From(12), ArticleId = articles[4].Id, TagId = tags[6].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ArticleTagId.From(13), ArticleId = articles[4].Id, TagId = tags[5].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ArticleTagId.From(14), ArticleId = articles[4].Id, TagId = tags[7].Id, CreatedAt = DateTime.UtcNow
            },
            // 角換わり -> 居飛車, 戦法
            new()
            {
                Id = ArticleTagId.From(15), ArticleId = articles[5].Id, TagId = tags[0].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ArticleTagId.From(16), ArticleId = articles[5].Id, TagId = tags[3].Id, CreatedAt = DateTime.UtcNow
            },
            // 三間飛車 -> 振り飛車, 戦法
            new()
            {
                Id = ArticleTagId.From(17), ArticleId = articles[6].Id, TagId = tags[1].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ArticleTagId.From(18), ArticleId = articles[6].Id, TagId = tags[3].Id, CreatedAt = DateTime.UtcNow
            },
            // 格言 -> 初心者向け
            new()
            {
                Id = ArticleTagId.From(19), ArticleId = articles[7].Id, TagId = tags[4].Id, CreatedAt = DateTime.UtcNow
            },
            // 実戦詰将棋 -> 詰将棋, 終盤
            new()
            {
                Id = ArticleTagId.From(20), ArticleId = articles[8].Id, TagId = tags[2].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ArticleTagId.From(21), ArticleId = articles[8].Id, TagId = tags[7].Id, CreatedAt = DateTime.UtcNow
            }
            // 将棋ソフトの記事にはタグを付けない(独立記事の例)
        };

        context.ArticleTags.AddRange(articleTags);
        await context.SaveChangesAsync();

        // 講座作成
        var courses = new List<Course>
        {
            new()
            {
                Id = CourseId.From(1),
                Name = "初心者のための将棋入門",
                UserId = users[3].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Id = CourseId.From(2),
                Name = "居飛車戦法マスター講座",
                UserId = users[0].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-8),
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                Id = CourseId.From(3),
                Name = "振り飛車完全攻略",
                UserId = users[1].Id,
                PublishedAt = DateTime.UtcNow.AddDays(-6),
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                UpdatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new()
            {
                Id = CourseId.From(4),
                Name = "詰将棋で学ぶ終盤力",
                UserId = users[2].Id,
                PublishedAt = null, // 未公開講座
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();

        // 講座に記事を紐付け
        articles[0].CourseId = courses[0].Id; // 棒銀入門 -> 初心者のための将棋入門
        articles[7].CourseId = courses[0].Id; // 格言 -> 初心者のための将棋入門
        articles[3].CourseId = courses[1].Id; // 矢倉戦法 -> 居飛車戦法マスター講座
        articles[5].CourseId = courses[1].Id; // 角換わり -> 居飛車戦法マスター講座
        articles[1].CourseId = courses[2].Id; // 四間飛車 -> 振り飛車完全攻略
        articles[6].CourseId = courses[2].Id; // 三間飛車 -> 振り飛車完全攻略
        articles[2].CourseId = courses[3].Id; // 詰将棋 -> 詰将棋で学ぶ終盤力
        articles[8].CourseId = courses[3].Id; // 実戦詰将棋 -> 詰将棋で学ぶ終盤力
        // articles[4] (王位戦感想) と articles[9] (将棋ソフト) は講座に紐付かない独立記事
        await context.SaveChangesAsync();

        // 講座へのいいね作成
        var courseLikes = new List<CourseLike>
        {
            // 初心者のための将棋入門
            new()
            {
                Id = CourseLikeId.From(1),
                UserId = users[0].Id,
                CourseId = courses[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                Id = CourseLikeId.From(2),
                UserId = users[1].Id,
                CourseId = courses[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new()
            {
                Id = CourseLikeId.From(3),
                UserId = users[2].Id,
                CourseId = courses[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },
            // 居飛車戦法マスター講座
            new()
            {
                Id = CourseLikeId.From(4),
                UserId = users[1].Id,
                CourseId = courses[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = CourseLikeId.From(5),
                UserId = users[3].Id,
                CourseId = courses[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            },
            // 振り飛車完全攻略
            new()
            {
                Id = CourseLikeId.From(6),
                UserId = users[0].Id,
                CourseId = courses[2].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = CourseLikeId.From(7),
                UserId = users[2].Id,
                CourseId = courses[2].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = CourseLikeId.From(8),
                UserId = users[3].Id,
                CourseId = courses[2].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        context.CourseLikes.AddRange(courseLikes);
        await context.SaveChangesAsync();

        // いいね作成
        var likes = new List<Like>
        {
            // 棒銀入門へのいいね
            new()
            {
                Id = LikeId.From(1), UserId = users[1].Id, ArticleId = articles[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new()
            {
                Id = LikeId.From(2), UserId = users[2].Id, ArticleId = articles[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = LikeId.From(3), UserId = users[3].Id, ArticleId = articles[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            // 四間飛車へのいいね
            new()
            {
                Id = LikeId.From(4), UserId = users[0].Id, ArticleId = articles[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = LikeId.From(5), UserId = users[2].Id, ArticleId = articles[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = LikeId.From(6), UserId = users[3].Id, ArticleId = articles[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            // 詰将棋へのいいね
            new()
            {
                Id = LikeId.From(7), UserId = users[0].Id, ArticleId = articles[2].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = LikeId.From(8), UserId = users[1].Id, ArticleId = articles[2].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            // 矢倉へのいいね
            new()
            {
                Id = LikeId.From(9), UserId = users[0].Id, ArticleId = articles[3].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = LikeId.From(10), UserId = users[1].Id, ArticleId = articles[3].Id, CreatedAt = DateTime.UtcNow
            },
            // 王位戦感想へのいいね
            new()
            {
                Id = LikeId.From(11), UserId = users[0].Id, ArticleId = articles[4].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = LikeId.From(12), UserId = users[2].Id, ArticleId = articles[4].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = LikeId.From(13), UserId = users[3].Id, ArticleId = articles[4].Id, CreatedAt = DateTime.UtcNow
            },
            // 角換わりへのいいね
            new()
            {
                Id = LikeId.From(14), UserId = users[1].Id, ArticleId = articles[5].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = LikeId.From(15), UserId = users[2].Id, ArticleId = articles[5].Id, CreatedAt = DateTime.UtcNow
            },
            // 三間飛車へのいいね
            new()
            {
                Id = LikeId.From(16), UserId = users[0].Id, ArticleId = articles[6].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = LikeId.From(17), UserId = users[3].Id, ArticleId = articles[6].Id, CreatedAt = DateTime.UtcNow
            },
            // 格言へのいいね
            new()
            {
                Id = LikeId.From(18), UserId = users[0].Id, ArticleId = articles[7].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = LikeId.From(19), UserId = users[1].Id, ArticleId = articles[7].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = LikeId.From(20), UserId = users[2].Id, ArticleId = articles[7].Id, CreatedAt = DateTime.UtcNow
            },
            // 実戦詰将棋へのいいね
            new()
            {
                Id = LikeId.From(21), UserId = users[0].Id, ArticleId = articles[8].Id, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = LikeId.From(22), UserId = users[1].Id, ArticleId = articles[8].Id, CreatedAt = DateTime.UtcNow
            },
            // 将棋ソフトへのいいね
            new()
            {
                Id = LikeId.From(23), UserId = users[1].Id, ArticleId = articles[9].Id, CreatedAt = DateTime.UtcNow
            }
        };

        context.Likes.AddRange(likes);
        await context.SaveChangesAsync();

        // コメント作成
        var comments = new List<Comment>
        {
            // 棒銀入門へのコメント
            new()
            {
                Id = CommentId.From(1),
                UserId = users[1].Id,
                ArticleId = articles[0].Id,
                Message = "棒銀はシンプルで分かりやすい戦法ですね！初心者の頃、よく使っていました。",
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new()
            {
                Id = CommentId.From(2),
                UserId = users[2].Id,
                ArticleId = articles[0].Id,
                Message = "角頭を攻めるときのタイミングが難しいです。もう少し詳しく知りたいです。",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            // 四間飛車へのコメント
            new()
            {
                Id = CommentId.From(3),
                UserId = users[0].Id,
                ArticleId = articles[1].Id,
                Message = "美濃囲いは本当に堅いですね。私も最初は振り飛車から入りました。",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            // 詰将棋へのコメント
            new()
            {
                Id = CommentId.From(4),
                UserId = users[3].Id,
                ArticleId = articles[2].Id,
                Message = "毎日継続することが大事なんですね。私も頑張ってみます！",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            // 角換わりへのコメント
            new()
            {
                Id = CommentId.From(5),
                UserId = users[1].Id,
                ArticleId = articles[5].Id,
                Message = "AI研究で定跡が変わりましたね。最新の形を勉強してみます！",
                CreatedAt = DateTime.UtcNow.AddHours(-11),
                UpdatedAt = DateTime.UtcNow.AddHours(-11)
            },
            // 三間飛車へのコメント
            new()
            {
                Id = CommentId.From(6),
                UserId = users[2].Id,
                ArticleId = articles[6].Id,
                Message = "石田流は憧れです。三間飛車を練習してみたいです。",
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-5)
            },
            // 格言へのコメント
            new()
            {
                Id = CommentId.From(7),
                UserId = users[0].Id,
                ArticleId = articles[7].Id,
                Message = "格言は本当に役立ちますね。初心者の頃、これらを意識して将棋が上達しました。",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = CommentId.From(8),
                UserId = users[1].Id,
                ArticleId = articles[7].Id,
                Message = "「王様は下段に落とせ」は詰将棋でも大事な考え方です！",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        context.Comments.AddRange(comments);
        await context.SaveChangesAsync();

        // フォロー関係作成
        var follows = new List<Follow>
        {
            // 羽生九段ファンが振り飛車党をフォロー
            new()
            {
                Id = FollowId.From(1),
                FollowerId = users[0].Id,
                FollowingId = users[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            },
            // 振り飛車党が詰将棋作家をフォロー
            new()
            {
                Id = FollowId.From(2),
                FollowerId = users[1].Id,
                FollowingId = users[2].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-8)
            },
            // 初心者将棋部が羽生九段ファンをフォロー
            new()
            {
                Id = FollowId.From(3),
                FollowerId = users[3].Id,
                FollowingId = users[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        context.Follows.AddRange(follows);
        await context.SaveChangesAsync();

        // 通知作成
        var notifications = new List<Notification>
        {
            // 羽生九段ファンの記事にいいねがついた通知
            new()
            {
                Id = NotificationId.From(1),
                UserId = users[0].Id,
                ArticleId = articles[3].Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            // 振り飛車党の記事にいいねがついた通知
            new()
            {
                Id = NotificationId.From(2),
                UserId = users[1].Id,
                ArticleId = articles[1].Id,
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ シードデータの投入が完了しました");
    }
}
