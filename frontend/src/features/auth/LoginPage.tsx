import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate } from 'react-router-dom';
import { loginSchema, type LoginFormValues } from './schemas';
import { login } from './api';
import { useAuthStore } from '../../shared/store/authStore';
import './auth.css';

export default function LoginPage() {
  const navigate = useNavigate();
  const setAuth = useAuthStore((s) => s.login);
  const [banner, setBanner] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });

  const onSubmit = async (values: LoginFormValues) => {
    setBanner(null);
    setSubmitting(true);

    try {
      const result = await login(values);

      if (!result.success || !result.data) {
        for (const err of result.errors) {
          const field = err.field.charAt(0).toLowerCase() + err.field.slice(1);
          if (field === 'email' || field === 'password') {
            setError(field as keyof LoginFormValues, { message: err.message });
          } else {
            setBanner(err.message);
          }
        }
        if (result.errors.length === 0) setBanner(result.message || 'Não foi possível entrar.');
        return;
      }

      setAuth(result.data.accessToken, { userId: '', email: values.email });
      navigate('/dashboard');
    } catch {
      setBanner('Não foi possível completar o login. Tente novamente.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-brand">
          <span className="auth-brand-mark">⚡</span>
          <h1>ChargePay</h1>
        </div>
        <p className="auth-subtitle">Entre na sua conta</p>

        {banner && <div className="form-banner-error">{banner}</div>}

        <form onSubmit={handleSubmit(onSubmit)} noValidate>
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
            <label className="form-label" htmlFor="password">Senha</label>
            <input
              id="password"
              type="password"
              className={`form-input ${errors.password ? 'error' : ''}`}
              placeholder="••••••"
              {...register('password')}
            />
            {errors.password && <span className="form-error-message">{errors.password.message}</span>}
          </div>

          <button type="submit" className="form-submit-button" disabled={submitting}>
            {submitting ? 'Entrando...' : 'Entrar'}
          </button>
        </form>

        <p className="form-footer-link">
          Ainda não tem conta? <Link to="/cadastro">Cadastre-se</Link>
        </p>
      </div>
    </div>
  );
}