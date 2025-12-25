import { NavLink, Outlet, Navigate } from 'react-router-dom';
import Header from '../components/common/Header';

const DashboardPage = () => {
  return (
    <div className="min-h-screen bg-background-light dark:bg-background-dark flex flex-col">
      <Header />

      <main className="flex-1 w-full max-w-[1440px] mx-auto p-6 md:p-8 flex flex-col gap-6">
        {/* Tabs */}
        <div className="border-b border-slate-200 dark:border-slate-700">
          <nav aria-label="Tabs" className="flex gap-8">
            <NavLink
              to="/dashboard/upload"
              className={({ isActive }) =>
                `group flex items-center gap-2 border-b-[3px] pb-3 px-1 text-sm font-bold transition-colors ${
                  isActive
                    ? 'border-primary text-primary'
                    : 'border-transparent text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200'
                }`
              }
            >
              <span className="material-symbols-outlined text-[20px]">cloud_upload</span>
              <span>Upload Invoices</span>
            </NavLink>

            <NavLink
              to="/dashboard/expenses"
              className={({ isActive }) =>
                `group flex items-center gap-2 border-b-[3px] pb-3 px-1 text-sm font-bold transition-colors ${
                  isActive
                    ? 'border-primary text-primary'
                    : 'border-transparent text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200'
                }`
              }
            >
              <span className="material-symbols-outlined text-[20px]">receipt_long</span>
              <span>Expenses Management</span>
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
