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
    <header className="sticky top-0 z-30 flex items-center justify-between whitespace-nowrap border-b border-solid border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 px-6 py-3 shadow-sm">
      <div className="flex items-center gap-3">
        <div className="flex items-center justify-center size-10 rounded-lg bg-primary/10 text-primary">
          <span className="material-symbols-outlined text-2xl">account_balance_wallet</span>
        </div>
        <h1 className="text-slate-900 dark:text-white text-lg font-bold leading-tight tracking-tight">
          {t('app.title')}
        </h1>
      </div>

      <div className="flex items-center gap-3">
        <LanguageSwitcher />
        <Button
          variant="ghost"
          size="sm"
          icon="logout"
          onClick={handleLogout}
        >
          {t('auth.logout')}
        </Button>
      </div>
    </header>
  );
};

export default Header;
