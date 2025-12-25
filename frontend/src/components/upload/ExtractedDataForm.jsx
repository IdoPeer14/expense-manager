import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Input from '../common/Input';
import Select from '../common/Select';
import Button from '../common/Button';
import { EXPENSE_CATEGORIES } from '../../utils/constants';

const expenseSchema = z.object({
  vendorName: z.string().min(1, 'Vendor name is required'),
  date: z.string().min(1, 'Date is required'),
  totalAmount: z.number().min(0, 'Amount must be positive'),
  currency: z.string().optional(),
  description: z.string().optional(),
  category: z.enum(['FOOD', 'VEHICLE', 'IT', 'OPERATIONS', 'TRAINING', 'OTHER']),
});

const ExtractedDataForm = ({ data, onSave, onDiscard, loading }) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm({
    resolver: zodResolver(expenseSchema),
    defaultValues: {
      vendorName: '',
      date: '',
      totalAmount: 0,
      currency: 'USD',
      description: '',
      category: 'OTHER',
    },
  });

  // Update form when data changes
  useEffect(() => {
    if (data) {
      reset({
        vendorName: data.vendorName || '',
        date: data.date || '',
        totalAmount: data.totalAmount || 0,
        currency: data.currency || 'USD',
        description: data.description || '',
        category: 'OTHER',
      });
    }
  }, [data, reset]);

  const onSubmit = (formData) => {
    onSave(formData);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Input
          label="Vendor Name"
          placeholder="Business name"
          error={errors.vendorName?.message}
          {...register('vendorName')}
        />

        <Input
          label="Date"
          type="date"
          error={errors.date?.message}
          {...register('date')}
        />

        <Input
          label="Total Amount"
          type="number"
          step="0.01"
          placeholder="0.00"
          error={errors.totalAmount?.message}
          {...register('totalAmount', { valueAsNumber: true })}
        />

        <Input
          label="Currency"
          placeholder="USD"
          error={errors.currency?.message}
          {...register('currency')}
        />

        <div className="md:col-span-2">
          <Select
            label="Category"
            options={EXPENSE_CATEGORIES}
            error={errors.category?.message}
            {...register('category')}
          />
        </div>

        <div className="md:col-span-2">
          <label className="block text-sm font-semibold text-slate-700 dark:text-slate-300 mb-2">
            Description
          </label>
          <textarea
            rows={3}
            placeholder="Service description or notes..."
            className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 px-4 py-2.5 text-slate-900 dark:text-white placeholder-slate-400 focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none transition-colors"
            {...register('description')}
          />
          {errors.description && (
            <p className="mt-1 text-sm text-red-600 dark:text-red-400">
              {errors.description.message}
            </p>
          )}
        </div>
      </div>

      <div className="flex gap-3 pt-2">
        <Button
          type="button"
          variant="secondary"
          onClick={onDiscard}
          disabled={loading}
          className="flex-1"
        >
          Discard
        </Button>
        <Button
          type="submit"
          variant="primary"
          loading={loading}
          className="flex-1"
        >
          Confirm & Save
        </Button>
      </div>
    </form>
  );
};

export default ExtractedDataForm;
