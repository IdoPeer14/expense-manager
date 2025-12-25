const ErrorAlert = ({ title = 'Error', message, onClose }) => {
  if (!message) return null;

  return (
    <div className="bg-red-50 dark:bg-red-900/10 border border-red-100 dark:border-red-900/20 rounded-lg p-3 flex gap-3 items-start">
      <span className="material-symbols-outlined text-red-600 dark:text-red-400 text-[20px] mt-0.5">
        error
      </span>
      <div className="flex-1 flex flex-col gap-0.5">
        <p className="text-xs font-semibold text-red-700 dark:text-red-400">{title}</p>
        <p className="text-xs text-red-600/80 dark:text-red-400/70">{message}</p>
      </div>
      {onClose && (
        <button
          onClick={onClose}
          className="text-red-400 hover:text-red-600 dark:hover:text-red-300 transition-colors"
        >
          <span className="material-symbols-outlined text-[18px]">close</span>
        </button>
      )}
    </div>
  );
};

export default ErrorAlert;
