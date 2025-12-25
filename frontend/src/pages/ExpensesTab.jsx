import { useState } from 'react';
import { useExpenses, useUpdateExpense, useDeleteExpense } from '../hooks/useExpenses';
import ExpenseFilters from '../components/expenses/ExpenseFilters';
import ExpenseTable from '../components/expenses/ExpenseTable';

const ExpensesTab = () => {
  const [filters, setFilters] = useState({});
  const { data, isLoading } = useExpenses(filters);
  const updateMutation = useUpdateExpense();
  const deleteMutation = useDeleteExpense();

  const expenses = data?.data || [];
  const pagination = data?.pagination || { totalCount: 0 };

  const handleApplyFilters = (newFilters) => {
    setFilters(newFilters);
  };

  const handleClearFilters = () => {
    setFilters({});
  };

  const handleCategoryChange = async (expenseId, category) => {
    await updateMutation.mutateAsync({
      expenseId,
      data: { category },
    });
  };

  const handleDelete = async (expenseId) => {
    if (confirm('Are you sure you want to delete this expense?')) {
      await deleteMutation.mutateAsync(expenseId);
    }
  };

  return (
    <div className="flex flex-col gap-6">
      {/* Filters Panel */}
      <section className="bg-white dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-800 p-5 shadow-sm">
        <div className="flex items-center gap-2 text-slate-800 dark:text-slate-200 mb-4">
          <span className="material-symbols-outlined text-[20px]">filter_list</span>
          <h2 className="text-base font-bold">Filter Expenses</h2>
        </div>
        <ExpenseFilters
          onApply={handleApplyFilters}
          onClear={handleClearFilters}
          loading={isLoading}
        />
      </section>

      {/* Expenses Table */}
      <section className="flex-1 flex flex-col bg-white dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-800 shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b border-slate-200 dark:border-slate-800 flex justify-between items-center">
          <p className="text-sm text-slate-500 dark:text-slate-400">
            Showing{' '}
            <span className="font-bold text-slate-900 dark:text-white">
              {pagination.totalCount}
            </span>{' '}
            transaction{pagination.totalCount !== 1 ? 's' : ''}
          </p>
        </div>

        <div className="flex-1">
          <ExpenseTable
            expenses={expenses}
            loading={isLoading}
            onCategoryChange={handleCategoryChange}
            onDelete={handleDelete}
          />
        </div>
      </section>
    </div>
  );
};

export default ExpensesTab;
