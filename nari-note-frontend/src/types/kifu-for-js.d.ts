declare module 'kifu-for-js' {
  import { ReactNode } from 'react';

  export interface KifuLiteProps {
    /**
     * 静的図面モード
     */
    static?: boolean;
    /**
     * BOD形式の将棋盤面テキスト
     */
    children?: ReactNode;
  }

  /**
   * kifu-for-js の静的図面コンポーネント
   */
  export const KifuLite: React.FC<KifuLiteProps>;
}
