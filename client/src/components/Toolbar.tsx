import React from 'react';

interface ToolbarProps {
  selectedCount: number;
  onBlock: () => void;
  onUnblock: () => void;
  onDelete: () => void;
  onDeleteUnverified: () => void;
}

const Toolbar: React.FC<ToolbarProps> = ({
  selectedCount,
  onBlock,
  onUnblock,
  onDelete,
  onDeleteUnverified
}) => {
  return (
    <div className="btn-toolbar mb-3 gap-2" role="toolbar">
      <button
        className="btn btn-danger"
        onClick={onBlock}
        disabled={selectedCount === 0}
        title="Block selected users"
      >
        <span className="bi bi-lock-fill me-1"></span>
        Block
      </button>

      <button
        className="btn btn-success"
        onClick={onUnblock}
        disabled={selectedCount === 0}
        title="Unblock selected users"
      >
        <span className="bi bi-unlock-fill me-1"></span>
        Unblock
      </button>

      <button
        className="btn btn-outline-danger"
        onClick={onDelete}
        disabled={selectedCount === 0}
        title="Delete selected users"
      >
        <span className="bi bi-trash-fill me-1"></span>
        Delete
      </button>

      <button
        className="btn btn-outline-warning"
        onClick={onDeleteUnverified}
        title="Delete all unverified users"
      >
        <span className="bi bi-person-x-fill me-1"></span>
        Delete Unverified
      </button>

      {selectedCount > 0 && (
        <span className="ms-2 align-self-center text-muted">
          {selectedCount} user{selectedCount !== 1 ? 's' : ''} selected
        </span>
      )}
    </div>
  );
};

export default Toolbar;