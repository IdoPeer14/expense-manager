import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../hooks/useAuth';
import { registerSchema } from '../utils/validation';
import Input from '../components/common/Input';
import Button from '../components/common/Button';
import ErrorAlert from '../components/common/ErrorAlert';

const RegisterPage = () => {
  const { t } = useTranslation();
  const [apiError, setApiError] = useState('');
  const { register: registerUser } = useAuth();
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (data) => {
    setApiError('');

    const result = await registerUser(data.email, data.password);

    if (result.success) {
      navigate('/dashboard/upload');
    } else {
      setApiError(result.error);
    }
  };

  return (
    <div className="min-h-screen flex flex-col items-center justify-center p-4 sm:p-6 lg:p-8 bg-background-light dark:bg-background-dark">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="flex justify-center mb-8">
          <div className="flex items-center gap-3">
            <div className="size-10 bg-primary/10 dark:bg-primary/20 rounded-lg flex items-center justify-center text-primary">
              <span className="material-symbols-outlined text-2xl">account_balance_wallet</span>
            </div>
            <h1 className="text-xl font-bold tracking-tight text-slate-900 dark:text-white">
              {t('auth.appTitle') || 'Expense Manager'}
            </h1>
          </div>
        </div>

        {/* Card */}
        <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl shadow-sm overflow-hidden">
          <div className="p-8">
            {/* Header */}
            <div className="mb-8 text-center">
              <h2 className="text-2xl font-bold mb-2 text-slate-900 dark:text-white">
                {t('auth.createAccountTitle')}
              </h2>
              <p className="text-slate-500 dark:text-slate-400 text-sm">
                {t('auth.registerSubtitle')}
              </p>
            </div>

            {/* Error Alert */}
            {apiError && (
              <div className="mb-6">
                <ErrorAlert
                  title={t('auth.registerFailed')}
                  message={apiError}
                  onClose={() => setApiError('')}
                />
              </div>
            )}

            {/* Form */}
            <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5">
              <Input
                label={t('auth.email')}
                type="email"
                placeholder="name@company.com"
                error={errors.email?.message}
                required
                {...register('email')}
              />

              <Input
                label={t('auth.password')}
                type="password"
                placeholder="••••••••"
                error={errors.password?.message}
                required
                {...register('password')}
              />

              <Input
                label={t('auth.confirmPassword')}
                type="password"
                placeholder="••••••••"
                error={errors.confirmPassword?.message}
                required
                {...register('confirmPassword')}
              />

              <Button
                type="submit"
                variant="primary"
                size="lg"
                loading={isSubmitting}
                className="w-full mt-2"
              >
                {t('auth.createAccount')}
              </Button>
            </form>
          </div>

          {/* Footer */}
          <div className="border-t border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-800 px-8 py-4 text-center">
            <p className="text-sm text-slate-600 dark:text-slate-400">
              {t('auth.haveAccount')}{' '}
              <Link
                to="/login"
                className="font-semibold text-primary hover:text-primary-hover hover:underline transition-colors"
              >
                {t('auth.loginHere')}
              </Link>
            </p>
          </div>
        </div>

        {/* Additional Links */}
        <div className="mt-8 flex justify-center gap-6 text-sm text-slate-500 dark:text-slate-400">
          <a href="#" className="hover:text-slate-900 dark:hover:text-slate-200 transition-colors">
            Privacy
          </a>
          <a href="#" className="hover:text-slate-900 dark:hover:text-slate-200 transition-colors">
            Terms
          </a>
          <a href="#" className="hover:text-slate-900 dark:hover:text-slate-200 transition-colors">
            Help
          </a>
        </div>
      </div>
    </div>
  );
};

export default RegisterPage;
