import React, { useState, useEffect } from 'react';
import { userService } from '../services/api';
import { User } from '../types/User';
import Toolbar from './Toolbar';

const UserTable: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [selectedUsers, setSelectedUsers] = useState<Set<number>>(new Set());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchUsers();
  }, []);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const response = await userService.getUsers();
      setUsers(response.data);
      setError('');
    } catch (err) {
      setError('Failed to load users');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSelectAll = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.checked) {
      setSelectedUsers(new Set(users.map(u => u.id)));
    } else {
      setSelectedUsers(new Set());
    }
  };

  const handleSelectUser = (userId: number) => {
    const newSelected = new Set(selectedUsers);
    if (newSelected.has(userId)) {
      newSelected.delete(userId);
    } else {
      newSelected.add(userId);
    }
    setSelectedUsers(newSelected);
  };

  const handleBlock = async () => {
    try {
      await userService.blockUsers(Array.from(selectedUsers));
      await fetchUsers();
      setSelectedUsers(new Set());
    } catch (err) {
      setError('Failed to block users');
    }
  };

  const handleUnblock = async () => {
    try {
      await userService.unblockUsers(Array.from(selectedUsers));
      await fetchUsers();
      setSelectedUsers(new Set());
    } catch (err) {
      setError('Failed to unblock users');
    }
  };

  const handleDelete = async () => {
    if (window.confirm('Are you sure you want to delete selected users?')) {
      try {
        await userService.deleteUsers(Array.from(selectedUsers));
        await fetchUsers();
        setSelectedUsers(new Set());
      } catch (err) {
        setError('Failed to delete users');
      }
    }
  };

  const handleDeleteUnverified = async () => {
    if (window.confirm('Are you sure you want to delete ALL unverified users?')) {
      try {
        await userService.deleteUnverified();
        await fetchUsers();
        setSelectedUsers(new Set());
      } catch (err) {
        setError('Failed to delete unverified users');
      }
    }
  };

  const formatDate = (dateString: string | null) => {
    if (!dateString) return 'Never';
    return new Date(dateString).toLocaleString();
  };

  if (loading) {
    return <div className="text-center">Loading users...</div>;
  }

  return (
    <div>
      <h2 className="mb-4">User Management</h2>
      
      {error && (
        <div className="alert alert-danger alert-dismissible fade show" role="alert">
          {error}
          <button type="button" className="btn-close" onClick={() => setError('')}></button>
        </div>
      )}

      <Toolbar
        selectedCount={selectedUsers.size}
        onBlock={handleBlock}
        onUnblock={handleUnblock}
        onDelete={handleDelete}
        onDeleteUnverified={handleDeleteUnverified}
      />

      <div className="table-responsive">
        <table className="table table-striped table-hover">
          <thead className="table-dark">
            <tr>
              <th style={{ width: '50px' }}>
                <div className="form-check">
                  <input
                    type="checkbox"
                    className="form-check-input"
                    checked={selectedUsers.size === users.length && users.length > 0}
                    onChange={handleSelectAll}
                  />
                </div>
              </th>
              <th>Name</th>
              <th>Email</th>
              <th>Last Login</th>
              <th>Status</th>
              <th>Registered</th>
            </tr>
          </thead>
          <tbody>
            {users.map(user => (
              <tr key={user.id}>
                <td>
                  <div className="form-check">
                    <input
                      type="checkbox"
                      className="form-check-input"
                      checked={selectedUsers.has(user.id)}
                      onChange={() => handleSelectUser(user.id)}
                    />
                  </div>
                </td>
                <td>{user.name}</td>
                <td>{user.email}</td>
                <td>{formatDate(user.lastLoginTime)}</td>
                <td>
                  <span className={`badge ${
                    user.status === 'active' ? 'bg-success' :
                    user.status === 'blocked' ? 'bg-danger' :
                    'bg-warning text-dark'
                  }`}>
                    {user.status}
                  </span>
                </td>
                <td>{formatDate(user.registrationTime)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {users.length === 0 && (
        <div className="alert alert-info">
          No users found.
        </div>
      )}
    </div>
  );
};

export default UserTable;