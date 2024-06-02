// Modal.js
import React, { useEffect, useState } from 'react';
import './css/Modal.css';

const Modal = ({ show, handleClose, handleSave, title, defaultValue }) => {
  const [inputValue, setInputValue] = useState(defaultValue);
  const [error, setError] = useState('');

  useEffect(() => {
    setInputValue(defaultValue);
    setError('');
  }, [defaultValue, show]);

  const onChange = (e) => {
    setInputValue(e.target.value);
    setError(''); // Clear error on input change
  };

  const validateFileName = (name) => {
    const regex = /^[\w\-. ]+$/;
    return regex.test(name);
  };

  const onSave = () => {
    if (validateFileName(inputValue)) {
      handleSave(inputValue);
    } else {
      setError('Nieprawidłowa nazwa pliku.');
    }
  };

  return (
    <div className={`modal ${show ? 'show' : ''}`}>
      <div className="modal-content">
        <h2>{title}</h2>
        <span className="close" onClick={handleClose}>&times;</span>
        <input type="text" value={inputValue} onChange={onChange} />
        {error && <p className="error">{error}</p>}
        <button onClick={onSave}>Zapisz</button>
      </div>
    </div>
  );
};

export default Modal;
