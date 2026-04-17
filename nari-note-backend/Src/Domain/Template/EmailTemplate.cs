namespace NariNoteBackend.Domain.Template;

public static class EmailTemplate
{
    public static string SignupVerificationHtml(string verificationUrl)
    {
        return $"""
                <!DOCTYPE html>
                <html lang="ja">
                <head>
                  <meta charset="UTF-8">
                  <meta name="viewport" content="width=device-width, initial-scale=1.0">
                  <title>メールアドレスの確認</title>
                </head>
                <body style="margin:0;padding:0;background-color:#f5f5f0;font-family:'Helvetica Neue',Arial,'Hiragino Kaku Gothic ProN',sans-serif;">
                  <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f5f5f0;padding:40px 0;">
                    <tr>
                      <td align="center">
                        <table width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%;">
                
                          <!-- Header -->
                          <tr>
                            <td style="background-color:#1a1a1a;border-radius:12px 12px 0 0;padding:32px 40px;text-align:center;">
                              <span style="font-size:28px;font-weight:700;color:#ffffff;letter-spacing:-0.5px;">なりノート</span>
                              <span style="display:block;font-size:12px;color:#888888;margin-top:4px;letter-spacing:2px;">NOTE YOUR THOUGHTS</span>
                            </td>
                          </tr>
                
                          <!-- Body -->
                          <tr>
                            <td style="background-color:#ffffff;padding:48px 40px;">
                              <h1 style="margin:0 0 16px;font-size:24px;font-weight:700;color:#1a1a1a;line-height:1.4;">
                                メールアドレスの確認
                              </h1>
                              <p style="margin:0 0 8px;font-size:15px;color:#555555;line-height:1.7;">
                                なりノートへご登録いただき、ありがとうございます。
                              </p>
                              <p style="margin:0 0 32px;font-size:15px;color:#555555;line-height:1.7;">
                                以下のボタンをクリックして、メールアドレスを確認してください。
                              </p>
                
                              <!-- CTA Button -->
                              <table width="100%" cellpadding="0" cellspacing="0">
                                <tr>
                                  <td align="center" style="padding:8px 0 40px;">
                                    <a href="{verificationUrl}"
                                       style="display:inline-block;background-color:#1a1a1a;color:#ffffff;text-decoration:none;font-size:15px;font-weight:600;padding:16px 48px;border-radius:8px;letter-spacing:0.3px;">
                                      メールアドレスを確認する
                                    </a>
                                  </td>
                                </tr>
                              </table>
                
                              <!-- Divider -->
                              <hr style="border:none;border-top:1px solid #eeeeee;margin:0 0 32px;">
                
                              <p style="margin:0 0 8px;font-size:13px;color:#888888;line-height:1.7;">
                                ボタンが機能しない場合は、以下のURLをブラウザに貼り付けてください：
                              </p>
                              <p style="margin:0;font-size:12px;color:#aaaaaa;word-break:break-all;line-height:1.6;">
                                {verificationUrl}
                              </p>
                            </td>
                          </tr>
                
                          <!-- Footer -->
                          <tr>
                            <td style="background-color:#f5f5f0;border-radius:0 0 12px 12px;padding:24px 40px;text-align:center;">
                              <p style="margin:0 0 8px;font-size:12px;color:#aaaaaa;line-height:1.6;">
                                このメールに心当たりがない場合は、無視していただいて構いません。
                              </p>
                              <p style="margin:0;font-size:12px;color:#aaaaaa;">
                                © 2025 nari-note. All rights reserved.
                              </p>
                            </td>
                          </tr>
                
                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>
                """;
    }

    public static string SignupVerificationText(string verificationUrl)
    {
        return $"""
                なりノートへご登録いただき、ありがとうございます。

                以下のURLにアクセスして、メールアドレスを確認してください：
                {verificationUrl}

                このメールに心当たりがない場合は、無視していただいて構いません。

                © 2025 nari-note
                """;
    }
}
