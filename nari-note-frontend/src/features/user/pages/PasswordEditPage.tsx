'use client';

import { useState } from 'react';
import { useUpdatePassword } from '@/lib/api';
import { PasswordEditTemplate } from '../templates/PasswordEditTemplate';

export function PasswordEditPage() {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [errors, setErrors] = useState<{
    currentPassword?: string;
    newPassword?: string;
    confirmPassword?: string;
  }>({});
  const [generalError, setGeneralError] = useState<string | undefined>();
  const [successMessage, setSuccessMessage] = useState<string | undefined>();

  const updatePassword = useUpdatePassword({
    onSuccess: () => {
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
      setErrors({});
      setGeneralError(undefined);
      setSuccessMessage('パスワードを変更しました');
    },
    onError: (err) => {
      setGeneralError(err instanceof Error ? err.message : 'パスワードの変更に失敗しました');
    },
  });

  const validateForm = (): boolean => {
    const newErrors: typeof errors = {};

    if (!currentPassword) {
      newErrors.currentPassword = '現在のパスワードを入力してください';
    }

    if (!newPassword) {
      newErrors.newPassword = '新しいパスワードを入力してください';
    } else if (newPassword.length < 8) {
      newErrors.newPassword = 'パスワードは8文字以上である必要があります';
    }

    if (!confirmPassword) {
      newErrors.confirmPassword = '確認用パスワードを入力してください';
    } else if (newPassword !== confirmPassword) {
      newErrors.confirmPassword = 'パスワードが一致しません';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setGeneralError(undefined);
    setSuccessMessage(undefined);

    if (!validateForm()) return;

    updatePassword.mutate({ currentPassword, newPassword });
  };

  return (
    <PasswordEditTemplate
      currentPassword={currentPassword}
      newPassword={newPassword}
      confirmPassword={confirmPassword}
      errors={errors}
      generalError={generalError}
      successMessage={successMessage}
      isSubmitting={updatePassword.isPending}
      onCurrentPasswordChange={setCurrentPassword}
      onNewPasswordChange={setNewPassword}
      onConfirmPasswordChange={setConfirmPassword}
      onSubmit={handleSubmit}
    />
  );
}
