import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Select from '../common/Select';
import { EXPENSE_CATEGORIES } from '../../utils/constants';
import { formatCurrency, formatDate } from '../../utils/formatters';

// Mobile Card Component
const ExpenseCard = ({ expense, onCategoryChange, onDelete, updating, deleting }) => {
  const { t } = useTranslation();
  const [category, setCategory] = useState(expense.category);

  const handleCategoryChange = async (e) => {
    const newCategory = e.target.value;
    setCategory(newCategory);
    await onCategoryChange(expense.id, newCategory);
  };

  // Generate avatar color from vendor name
  const getAvatarColor = (name) => {
    const colors = [
      'bg-blue-500',
      'bg-green-500',
      'bg-yellow-500',
      'bg-red-500',
      'bg-purple-500',
      'bg-pink-500',
    ];
    const charCode = name.charCodeAt(0) || 0;
    return colors[charCode % colors.length];
  };

  const getInitials = (name) => {
    return name
      .split(' ')
      .map((word) => word[0])
      .join('')
      .substring(0, 2)
      .toUpperCase();
  };

  return (
    <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-700 p-4 hover:shadow-md transition-shadow">
      {/* Header with vendor and amount */}
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-3 flex-1">
          <div
            className={`flex items-center justify-center w-10 h-10 rounded-full text-white font-bold text-sm flex-shrink-0 ${getAvatarColor(
              expense.vendorName || t('expenses.unknownVendor')
            )}`}
          >
            {getInitials(expense.vendorName || 'UK')}
          </div>
          <div className="flex-1 min-w-0">
            <p className="font-semibold text-slate-900 dark:text-white truncate">
              {expense.vendorName || t('expenses.unknownVendor')}
            </p>
            <p className="text-xs text-slate-500 dark:text-slate-400">
              {formatDate(expense.date)}
            </p>
          </div>
        </div>
        <div className="text-right flex-shrink-0 ml-2">
          <p className="font-mono font-bold text-slate-900 dark:text-white">
            {formatCurrency(expense.totalAmount)}
          </p>
        </div>
      </div>

      {/* Invoice number */}
      {expense.invoiceNumber && (
        <div className="mb-3 text-xs text-slate-600 dark:text-slate-400">
          <span className="font-medium">Invoice #:</span> {expense.invoiceNumber}
        </div>
      )}

      {/* Category and actions */}
      <div className="flex items-center gap-2">
        <select
          value={category}
          onChange={handleCategoryChange}
          disabled={updating}
          className="flex-1 rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-3 py-2 text-sm text-slate-900 dark:text-white focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none transition-colors disabled:opacity-50"
        >
          {EXPENSE_CATEGORIES.map((cat) => (
            <option key={cat.value} value={cat.value}>
              {cat.label}
            </option>
          ))}
        </select>
        <button
          onClick={() => onDelete(expense.id)}
          disabled={deleting}
          className="flex items-center justify-center w-10 h-10 rounded-lg text-red-600 hover:bg-red-50 dark:text-red-400 dark:hover:bg-red-900/20 transition-colors disabled:opacity-50"
          title={t('expenses.deleteTitle')}
        >
          <span className="material-symbols-outlined text-[20px]">delete</span>
        </button>
      </div>
    </div>
  );
};

// Desktop Table Row Component
const ExpenseTableRow = ({ expense, onCategoryChange, onDelete, updating, deleting }) => {
  const { t } = useTranslation();
  const [category, setCategory] = useState(expense.category);

  const handleCategoryChange = async (e) => {
    const newCategory = e.target.value;
    setCategory(newCategory);
    await onCategoryChange(expense.id, newCategory);
  };

  // Generate avatar color from vendor name
  const getAvatarColor = (name) => {
    const colors = [
      'bg-blue-500',
      'bg-green-500',
      'bg-yellow-500',
      'bg-red-500',
      'bg-purple-500',
      'bg-pink-500',
    ];
    const charCode = name.charCodeAt(0) || 0;
    return colors[charCode % colors.length];
  };

  const getInitials = (name) => {
    return name
      .split(' ')
      .map((word) => word[0])
      .join('')
      .substring(0, 2)
      .toUpperCase();
  };

  return (
    <tr className="hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors">
      <td className="px-6 py-4 whitespace-nowrap">
        <div className="flex items-center gap-3">
          <div
            className={`flex items-center justify-center w-10 h-10 rounded-full text-white font-bold text-sm ${getAvatarColor(
              expense.vendorName || t('expenses.unknownVendor')
            )}`}
          >
            {getInitials(expense.vendorName || 'UK')}
          </div>
          <span className="font-medium text-slate-900 dark:text-white">
            {expense.vendorName || t('expenses.unknownVendor')}
          </span>
        </div>
      </td>
      <td className="px-6 py-4 whitespace-nowrap text-slate-700 dark:text-slate-300">
        {formatDate(expense.date)}
      </td>
      <td className="px-6 py-4 whitespace-nowrap font-mono text-slate-900 dark:text-white">
        {formatCurrency(expense.totalAmount)}
      </td>
      <td className="px-6 py-4 whitespace-nowrap text-slate-700 dark:text-slate-300">
        {expense.invoiceNumber || '-'}
      </td>
      <td className="px-6 py-4 whitespace-nowrap">
        <select
          value={category}
          onChange={handleCategoryChange}
          disabled={updating}
          className="rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-3 py-1.5 text-sm text-slate-900 dark:text-white focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none transition-colors disabled:opacity-50"
        >
          {EXPENSE_CATEGORIES.map((cat) => (
            <option key={cat.value} value={cat.value}>
              {cat.label}
            </option>
          ))}
        </select>
      </td>
      <td className="px-6 py-4 whitespace-nowrap text-right">
        <button
          onClick={() => onDelete(expense.id)}
          disabled={deleting}
          className="text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300 transition-colors disabled:opacity-50"
          title={t('expenses.deleteTitle')}
        >
          <span className="material-symbols-outlined text-[20px]">delete</span>
        </button>
      </td>
    </tr>
  );
};

const ExpenseTable = ({ expenses, loading, onCategoryChange, onDelete }) => {
  const [updatingId, setUpdatingId] = useState(null);
  const [deletingId, setDeletingId] = useState(null);

  const handleCategoryChange = async (id, category) => {
    setUpdatingId(id);
    try {
      await onCategoryChange(id, category);
    } finally {
      setUpdatingId(null);
    }
  };

  const handleDelete = async (id) => {
    setDeletingId(id);
    try {
      await onDelete(id);
    } finally {
      setDeletingId(null);
    }
  };

  if (loading) {
    return (
      <div className="animate-pulse">
        {[1, 2, 3, 4, 5].map((i) => (
          <div
            key={i}
            className="h-16 bg-slate-200 dark:bg-slate-800 mb-2 rounded"
          ></div>
        ))}
      </div>
    );
  }

  if (!expenses || expenses.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <span className="material-symbols-outlined text-6xl text-slate-300 dark:text-slate-600 mb-4">
          receipt_long
        </span>
        <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-2">
          No expenses found
        </h3>
        <p className="text-sm text-slate-500 dark:text-slate-400 max-w-sm">
          Try adjusting your filters or upload a new invoice
        </p>
      </div>
    );
  }

  return (
    <>
      {/* Mobile Card View */}
      <div className="md:hidden space-y-3">
        {expenses.map((expense) => (
          <ExpenseCard
            key={expense.id}
            expense={expense}
            onCategoryChange={handleCategoryChange}
            onDelete={handleDelete}
            updating={updatingId === expense.id}
            deleting={deletingId === expense.id}
          />
        ))}
      </div>

      {/* Desktop Table View */}
      <div className="hidden md:block overflow-x-auto">
        <table className="w-full">
          <thead className="bg-slate-50 dark:bg-slate-800/50 border-b border-slate-200 dark:border-slate-700">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider">
                Vendor
              </th>
              <th className="px-6 py-3 text-left text-xs font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider">
                Date
              </th>
              <th className="px-6 py-3 text-left text-xs font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider">
                Amount
              </th>
              <th className="px-6 py-3 text-left text-xs font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider">
                Invoice #
              </th>
              <th className="px-6 py-3 text-left text-xs font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider">
                Category
              </th>
              <th className="px-6 py-3 text-right text-xs font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
            {expenses.map((expense) => (
              <ExpenseTableRow
                key={expense.id}
                expense={expense}
                onCategoryChange={handleCategoryChange}
                onDelete={handleDelete}
                updating={updatingId === expense.id}
                deleting={deletingId === expense.id}
              />
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
};

export default ExpenseTable;
