import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate } from 'react-router-dom';
import { cadastroSchema, type CadastroFormValues } from './schemas';
import { cadastrar } from './api';
import { maskDocument, maskPhone, onlyDigits } from '../../shared/lib/mask';
import { calculateAge } from '../../shared/lib/date';
import './auth.css';

export default function CadastroPage() {
  const navigate = useNavigate();
  const [banner, setBanner] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [ageWarning, setAgeWarning] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    setValue,
    watch,
    formState: { errors },
  } = useForm<CadastroFormValues>({ resolver: zodResolver(cadastroSchema) });

  const documentValue = watch('document') || '';
  const phoneValue = watch('phone') || '';

  const handleBirthDateBlur = (value: string) => {
    if (!value) return;
    const age = calculateAge(value);
    setAgeWarning(age < 18 ? 'IDADE MENOS QUE 18 ANOS' : null);
  };

  const onSubmit = async (values: CadastroFormValues) => {
    setBanner(null);
    setSubmitting(true);

    try {
      const result = await cadastrar({
        ...values,
        document: onlyDigits(values.document),
        phone: onlyDigits(values.phone),
      });

      if (!result.success || !result.data) {
        let hasFieldError = false;
        for (const err of result.errors) {
          const field = err.field.charAt(0).toLowerCase() + err.field.slice(1);
          if (field in ({} as CadastroFormValues) || ['firstName','lastName','document','phone','email','birthDate','password'].includes(field)) {
            setError(field as keyof CadastroFormValues, { message: err.message });
            hasFieldError = true;
          } else {
            setBanner(err.message);
          }
        }
        if (!hasFieldError && result.errors.length === 0) {
          setBanner(result.message || 'Não foi possível completar o cadastro.');
        }
        return;
      }

      navigate('/login');
    } catch {
      setBanner('Não foi possível completar o cadastro. Tente novamente.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card auth-card-wide">
        <div className="auth-brand">
          <span className="auth-brand-mark">⚡</span>
          <h1>ChargePay</h1>
        </div>
        <p className="auth-subtitle">Crie sua conta</p>

        {banner && <div className="form-banner-error">{banner}</div>}

        <form onSubmit={handleSubmit(onSubmit)} noValidate>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label" htmlFor="firstName">Primeiro nome</label>
              <input
                id="firstName"
                className={`form-input ${errors.firstName ? 'error' : ''}`}
                placeholder="Michael"
                {...register('firstName')}
              />
              {errors.firstName && <span className="form-error-message">{errors.firstName.message}</span>}
            </div>

            <div className="form-group">
              <label className="form-label" htmlFor="lastName">Sobrenome</label>
              <input
                id="lastName"
                className={`form-input ${errors.lastName ? 'error' : ''}`}
                placeholder="Neto"
                {...register('lastName')}
              />
              {errors.lastName && <span className="form-error-message">{errors.lastName.message}</span>}
            </div>
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="document">CPF ou CNPJ</label>
            <input
              id="document"
              className={`form-input ${errors.document ? 'error' : ''}`}
              placeholder="000.000.000-00"
              value={maskDocument(documentValue)}
              onChange={(e) => setValue('document', e.target.value, { shouldValidate: true })}
            />
            {errors.document && <span className="form-error-message">{errors.document.message}</span>}
          </div>

          <div className="form-row">
            <div className="form-group">
              <label className="form-label" htmlFor="birthDate">Data de nascimento</label>
              <input
                id="birthDate"
                type="date"
                className={`form-input ${errors.birthDate || ageWarning ? 'error' : ''}`}
                {...register('birthDate', {
                  onBlur: (e) => handleBirthDateBlur(e.target.value),
                })}
              />
              {ageWarning && <span className="form-error-message">{ageWarning}</span>}
              {!ageWarning && errors.birthDate && (
                <span className="form-error-message">{errors.birthDate.message}</span>
              )}
            </div>

            <div className="form-group">
              <label className="form-label" htmlFor="phone">Telefone</label>
              <input
                id="phone"
                className={`form-input ${errors.phone ? 'error' : ''}`}
                placeholder="(00) 00000-0000"
                value={maskPhone(phoneValue)}
                onChange={(e) => setValue('phone', e.target.value, { shouldValidate: true })}
              />
              {errors.phone && <span className="form-error-message">{errors.phone.message}</span>}
            </div>
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="email">E-mail</label>
            <input
              id="email"
              type="email"
              className={`form-input ${errors.email ? 'error' : ''}`}
              placeholder="seu@email.com"
              {...register('email')}
            />
            {errors.email && <span className="form-error-message">{errors.email.message}</span>}
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="password">Senha (6 dígitos)</label>
            <input
              id="password"
              type="password"
              inputMode="numeric"
              maxLength={6}
              className={`form-input ${errors.password ? 'error' : ''}`}
              placeholder="••••••"
              {...register('password')}
            />
            {errors.password && <span className="form-error-message">{errors.password.message}</span>}
          </div>

          <button type="submit" className="form-submit-button" disabled={submitting}>
            {submitting ? 'Criando conta...' : 'Criar conta'}
          </button>
        </form>

        <p className="form-footer-link">
          Já tem conta? <Link to="/login">Entrar</Link>
        </p>
      </div>
    </div>
  );
}