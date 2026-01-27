interface FormTitleProps {
  children: React.ReactNode;
}

/**
 * FormTitle - Atom Component
 * 
 * フォームのタイトル
 */
export function FormTitle({ children }: FormTitleProps) {
  return (
    <h1 className="text-3xl font-bold text-brand-text mb-6 text-center" style={{ fontFamily: 'serif' }}>
      {children}
    </h1>
  );
}
