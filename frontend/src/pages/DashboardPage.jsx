import { NavLink, Outlet, Navigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Header from '../components/common/Header';

const DashboardPage = () => {
  const { t } = useTranslation();

  return (
    <div className="min-h-screen bg-background-light dark:bg-background-dark flex flex-col">
      <Header />

      <main className="flex-1 w-full max-w-[1440px] mx-auto p-3 sm:p-6 md:p-8 flex flex-col gap-4 sm:gap-6">
        {/* Tabs */}
        <div className="border-b border-slate-200 dark:border-slate-700">
          <nav aria-label="Tabs" className="flex gap-4 sm:gap-8">
            <NavLink
              to="/dashboard/upload"
              className={({ isActive }) =>
                `group flex items-center gap-1.5 sm:gap-2 border-b-[3px] pb-2 sm:pb-3 px-1 text-xs sm:text-sm font-bold transition-colors ${
                  isActive
                    ? 'border-primary text-primary'
                    : 'border-transparent text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200'
                }`
              }
            >
              <span className="material-symbols-outlined text-[18px] sm:text-[20px]">cloud_upload</span>
              <span>{t('dashboard.uploadTab')}</span>
            </NavLink>

            <NavLink
              to="/dashboard/expenses"
              className={({ isActive }) =>
                `group flex items-center gap-1.5 sm:gap-2 border-b-[3px] pb-2 sm:pb-3 px-1 text-xs sm:text-sm font-bold transition-colors ${
                  isActive
                    ? 'border-primary text-primary'
                    : 'border-transparent text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200'
                }`
              }
            >
              <span className="material-symbols-outlined text-[18px] sm:text-[20px]">receipt_long</span>
              <span>{t('dashboard.expensesTab')}</span>
            </NavLink>
          </nav>
        </div>

        {/* Tab Content */}
        <Outlet />
      </main>
    </div>
  );
};

export default DashboardPage;
