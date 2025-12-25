import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../hooks/useAuth';
import { loginSchema } from '../utils/validation';
import Input from '../components/common/Input';
import Button from '../components/common/Button';
import ErrorAlert from '../components/common/ErrorAlert';
import LanguageSwitcher from '../components/common/LanguageSwitcher';

const LoginPage = () => {
  const { t } = useTranslation();
  const [apiError, setApiError] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data) => {
    setApiError('');

    const result = await login(data.email, data.password);

    if (result.success) {
      navigate('/dashboard/upload');
    } else {
      setApiError(result.error);
    }
  };

  return (
    <div className="min-h-screen flex flex-col items-center justify-center p-4 sm:p-6 lg:p-8 bg-background-light dark:bg-background-dark relative overflow-hidden">
      {/* Decorative Background */}
      <div className="absolute top-0 left-0 w-full h-full overflow-hidden pointer-events-none z-0">
        <div className="absolute top-[-10%] right-[-5%] w-[500px] h-[500px] bg-primary/5 dark:bg-primary/10 rounded-full blur-3xl opacity-70"></div>
        <div className="absolute bottom-[-10%] left-[-10%] w-[600px] h-[600px] bg-blue-400/5 dark:bg-blue-600/10 rounded-full blur-3xl opacity-70"></div>
      </div>

      {/* Language Switcher - Top Right */}
      <div className="absolute top-6 ltr:right-6 rtl:left-6 z-20">
        <LanguageSwitcher />
      </div>

      {/* Login Card */}
      <div className="w-full max-w-[440px] bg-white dark:bg-slate-900 rounded-xl shadow-lg border border-slate-100 dark:border-slate-800 z-10 relative overflow-hidden">
        <div className="p-8 sm:p-10 flex flex-col gap-6">
          {/* Header */}
          <div className="flex flex-col items-center text-center gap-2">
            <div className="size-12 bg-primary/10 dark:bg-primary/20 rounded-xl flex items-center justify-center mb-2 text-primary">
              <span className="material-symbols-outlined text-[28px]">account_balance_wallet</span>
            </div>
            <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">
              {t('auth.welcomeBack')}
            </h1>
            <p className="text-slate-500 dark:text-slate-400 text-sm">
              {t('auth.loginSubtitle')}
            </p>
          </div>

          {/* Error Message */}
          {apiError && (
            <ErrorAlert
              title={t('auth.loginFailed')}
              message={apiError}
              onClose={() => setApiError('')}
            />
          )}

          {/* Form */}
          <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
            <Input
              label={t('auth.email')}
              type="email"
              placeholder="name@company.com"
              icon="mail"
              error={errors.email?.message}
              {...register('email')}
            />

            <Input
              label={t('auth.password')}
              type="password"
              placeholder="••••••••"
              icon="lock"
              error={errors.password?.message}
              {...register('password')}
            />

            <div className="pt-2">
              <Button
                type="submit"
                variant="primary"
                size="lg"
                loading={isSubmitting}
                className="w-full"
              >
                {t('auth.signIn')}
              </Button>
            </div>
          </form>
        </div>

        {/* Footer */}
        <div className="bg-slate-50 dark:bg-slate-800 px-8 py-4 border-t border-slate-100 dark:border-slate-700 text-center">
          <p className="text-sm text-slate-600 dark:text-slate-400">
            {t('auth.newAccount')}{' '}
            <Link
              to="/register"
              className="font-semibold text-primary hover:text-primary-hover transition-colors"
            >
              {t('auth.createAccount')}
            </Link>
          </p>
        </div>
      </div>

      {/* Bottom Links */}
      <div className="mt-8 flex gap-6 text-sm text-slate-500 dark:text-slate-400 z-10">
        <a href="#" className="hover:text-slate-900 dark:hover:text-slate-200 transition-colors">
          Privacy Policy
        </a>
        <a href="#" className="hover:text-slate-900 dark:hover:text-slate-200 transition-colors">
          Terms of Service
        </a>
        <a href="#" className="hover:text-slate-900 dark:hover:text-slate-200 transition-colors">
          Contact Support
        </a>
      </div>
    </div>
  );
};

export default LoginPage;
