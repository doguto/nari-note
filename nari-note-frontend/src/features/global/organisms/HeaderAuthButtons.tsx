import Link from 'next/link';


export function HeaderAuthButtons() {
  return (
    <>
      <Link
        href="/login"
        className="text-white hover:text-brand-primary transition-colors text-sm"
        style={{ fontFamily: 'serif' }}
      >
        ログイン
      </Link>
    </>
  );
}
