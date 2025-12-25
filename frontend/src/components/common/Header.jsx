import { useTranslation } from 'react-i18next';
import { useAuth } from '../../hooks/useAuth';
import { useNavigate } from 'react-router-dom';
import Button from './Button';
import LanguageSwitcher from './LanguageSwitcher';

const Header = () => {
  const { t } = useTranslation();
  const { logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <header className="sticky top-0 z-30 flex items-center justify-between whitespace-nowrap border-b border-solid border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 px-3 sm:px-6 py-3 shadow-sm">
      <div className="flex items-center gap-2 sm:gap-3">
        <div className="flex items-center justify-center size-8 sm:size-10 rounded-lg bg-primary/10 text-primary">
          <span className="material-symbols-outlined text-xl sm:text-2xl">account_balance_wallet</span>
        </div>
        <h1 className="text-slate-900 dark:text-white text-base sm:text-lg font-bold leading-tight tracking-tight">
          {t('app.title')}
        </h1>
      </div>

      <div className="flex items-center gap-2 sm:gap-3">
        <LanguageSwitcher />
        <Button
          variant="ghost"
          size="sm"
          icon="logout"
          onClick={handleLogout}
          className="hidden sm:inline-flex"
        >
          {t('auth.logout')}
        </Button>
        {/* Icon-only logout button for mobile */}
        <button
          onClick={handleLogout}
          className="sm:hidden flex items-center justify-center w-9 h-9 rounded-lg text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
          title={t('auth.logout')}
        >
          <span className="material-symbols-outlined text-[20px]">logout</span>
        </button>
      </div>
    </header>
  );
};

export default Header;
