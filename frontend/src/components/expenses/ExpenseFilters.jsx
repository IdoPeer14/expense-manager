import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Input from '../common/Input';
import Select from '../common/Select';
import Button from '../common/Button';
import { EXPENSE_CATEGORIES } from '../../utils/constants';

const ExpenseFilters = ({ onApply, onClear, loading }) => {
  const { t } = useTranslation();
  const [filters, setFilters] = useState({
    dateRange: 'last30',
    startDate: '',
    endDate: '',
    minAmount: '',
    maxAmount: '',
    category: '',
    businessName: '',
  });

  const handleChange = (field, value) => {
    setFilters((prev) => ({ ...prev, [field]: value }));
  };

  const handleApply = () => {
    // Calculate date range based on selection
    const now = new Date();
    let startDate = filters.startDate;
    let endDate = filters.endDate;

    if (filters.dateRange === 'last30') {
      const thirtyDaysAgo = new Date(now);
      thirtyDaysAgo.setDate(now.getDate() - 30);
      startDate = thirtyDaysAgo.toISOString().split('T')[0];
      endDate = now.toISOString().split('T')[0];
    } else if (filters.dateRange === 'thisMonth') {
      startDate = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0];
      endDate = now.toISOString().split('T')[0];
    } else if (filters.dateRange === 'lastQuarter') {
      const threeMonthsAgo = new Date(now);
      threeMonthsAgo.setMonth(now.getMonth() - 3);
      startDate = threeMonthsAgo.toISOString().split('T')[0];
      endDate = now.toISOString().split('T')[0];
    }

    const params = {
      startDate: startDate || undefined,
      endDate: endDate || undefined,
      minAmount: filters.minAmount ? parseFloat(filters.minAmount) : undefined,
      maxAmount: filters.maxAmount ? parseFloat(filters.maxAmount) : undefined,
      category: filters.category || undefined,
      businessName: filters.businessName || undefined,
    };

    onApply(params);
  };

  const handleClear = () => {
    setFilters({
      dateRange: 'last30',
      startDate: '',
      endDate: '',
      minAmount: '',
      maxAmount: '',
      category: '',
      businessName: '',
    });
    onClear();
  };

  // Translate date range options
  const translatedDateRangeOptions = [
    { value: 'last30', label: t('dateRanges.last30') },
    { value: 'thisMonth', label: t('dateRanges.thisMonth') },
    { value: 'lastQuarter', label: t('dateRanges.lastQuarter') },
    { value: 'custom', label: t('dateRanges.custom') },
  ];

  // Translate category options
  const translatedCategories = EXPENSE_CATEGORIES.map(cat => ({
    value: cat.value,
    label: t(`categories.${cat.value}`)
  }));

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <Select
          label={t('expenses.dateRange')}
          options={translatedDateRangeOptions}
          value={filters.dateRange}
          onChange={(e) => handleChange('dateRange', e.target.value)}
        />

        {filters.dateRange === 'custom' && (
          <>
            <Input
              label={t('dateRanges.startDate')}
              type="date"
              value={filters.startDate}
              onChange={(e) => handleChange('startDate', e.target.value)}
            />
            <Input
              label={t('dateRanges.endDate')}
              type="date"
              value={filters.endDate}
              onChange={(e) => handleChange('endDate', e.target.value)}
            />
          </>
        )}

        <Input
          label={t('expenses.minAmount')}
          type="number"
          step="0.01"
          placeholder="0.00"
          value={filters.minAmount}
          onChange={(e) => handleChange('minAmount', e.target.value)}
        />

        <Input
          label={t('expenses.maxAmount')}
          type="number"
          step="0.01"
          placeholder="0.00"
          value={filters.maxAmount}
          onChange={(e) => handleChange('maxAmount', e.target.value)}
        />

        <Select
          label={t('expenses.category')}
          options={[{ value: '', label: t('categories.all') }, ...translatedCategories]}
          value={filters.category}
          onChange={(e) => handleChange('category', e.target.value)}
        />

        <Input
          label={t('expenses.businessName')}
          placeholder={t('expenses.searchPlaceholder')}
          value={filters.businessName}
          onChange={(e) => handleChange('businessName', e.target.value)}
        />
      </div>

      <div className="flex gap-3">
        <Button
          type="button"
          variant="primary"
          onClick={handleApply}
          loading={loading}
        >
          {t('expenses.applyFilters')}
        </Button>
        <Button type="button" variant="secondary" onClick={handleClear}>
          {t('expenses.clearFilters')}
        </Button>
      </div>
    </div>
  );
};

export default ExpenseFilters;
