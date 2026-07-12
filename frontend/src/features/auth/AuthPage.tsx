// import { type ChangeEvent, type FormEvent, useState } from 'react';
// import { useNavigate } from 'react-router-dom';
// import axios from 'axios';
// import { apiClient } from '../../shared/api/client';

// export function AuthPage() {
//   const navigate = useNavigate();
//   const [form, setForm] = useState({
//     firstName: '',
//     lastName: '',
//     document: '',
//     phone: '',
//     email: '',
//     birthDate: '',
//     password: ''
//   });
//   const [message, setMessage] = useState<string | null>(null);
//   const [error, setError] = useState<string | null>(null);
//   const [isSubmitting, setIsSubmitting] = useState(false);

//   const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
//     const { name, value } = event.target;
//     setForm((current) => ({ ...current, [name]: value }));
//   };

//   const handleSubmit = async (event: FormEvent) => {
//     event.preventDefault();
//     setIsSubmitting(true);
//     setMessage(null);
//     setError(null);

//     try {
//       const response = await apiClient.post('/api/users', {
//         ...form,
//         birthDate: new Date(form.birthDate).toISOString()
//       });

//       const payload = response.data;
//       if (payload?.success) {
//         setMessage(payload.message ?? 'Cadastro realizado com sucesso.');
//         navigate('/onboarding');
//         return;
//       }

//       setError(payload?.errors?.[0]?.message ?? 'Não foi possível concluir o cadastro.');
//     } catch (err: unknown) {
//       if (axios.isAxiosError(err) && err.response?.data?.errors?.length) {
//         setError(err.response.data.errors[0].message);
//       } else {
//         setError('Não foi possível conectar ao backend. Verifique se a API está em execução.');
//       }
//     } finally {
//       setIsSubmitting(false);
//     }
//   };

//   return (
//     <div style={{ minHeight: '100vh', display: 'grid', placeItems: 'center', background: '#0f172a', color: 'white', fontFamily: 'Inter, sans-serif' }}>
//       <div style={{ width: '100%', maxWidth: 560, padding: 32, borderRadius: 20, background: 'rgba(15, 23, 42, 0.9)', boxShadow: '0 20px 50px rgba(0,0,0,0.3)' }}>
//         <h1 style={{ fontSize: 32, marginBottom: 8 }}>ChargePay</h1>
//         <p style={{ color: '#94a3b8', marginBottom: 24 }}>Cadastro real integrado ao endpoint de criação de usuário.</p>
//         <form onSubmit={handleSubmit} style={{ display: 'grid', gap: 12 }}>
//           <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
//             <div>
//               <label style={{ display: 'block', marginBottom: 8 }}>Nome</label>
//               <input name="firstName" value={form.firstName} onChange={handleChange} required style={{ width: '100%', padding: 12, borderRadius: 10, border: '1px solid #334155' }} />
//             </div>
//             <div>
//               <label style={{ display: 'block', marginBottom: 8 }}>Sobrenome</label>
//               <input name="lastName" value={form.lastName} onChange={handleChange} required style={{ width: '100%', padding: 12, borderRadius: 10, border: '1px solid #334155' }} />
//             </div>
//           </div>
//           <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
//             <div>
//               <label style={{ display: 'block', marginBottom: 8 }}>Documento</label>
//               <input name="document" value={form.document} onChange={handleChange} required style={{ width: '100%', padding: 12, borderRadius: 10, border: '1px solid #334155' }} />
//             </div>
//             <div>
//               <label style={{ display: 'block', marginBottom: 8 }}>Telefone</label>
//               <input name="phone" value={form.phone} onChange={handleChange} required style={{ width: '100%', padding: 12, borderRadius: 10, border: '1px solid #334155' }} />
//             </div>
//           </div>
//           <label style={{ display: 'block', marginBottom: 8 }}>E-mail</label>
//           <input name="email" type="email" value={form.email} onChange={handleChange} required style={{ width: '100%', padding: 12, borderRadius: 10, border: '1px solid #334155' }} />
//           <label style={{ display: 'block', marginBottom: 8 }}>Data de nascimento</label>
//           <input name="birthDate" type="date" value={form.birthDate} onChange={handleChange} required style={{ width: '100%', padding: 12, borderRadius: 10, border: '1px solid #334155' }} />
//           <label style={{ display: 'block', marginBottom: 8 }}>Senha (6 dígitos)</label>
//           <input name="password" type="password" value={form.password} onChange={handleChange} required maxLength={6} style={{ width: '100%', padding: 12, borderRadius: 10, border: '1px solid #334155' }} />
//           <button disabled={isSubmitting} style={{ width: '100%', padding: 12, borderRadius: 10, border: 'none', background: '#3b82f6', color: 'white', cursor: 'pointer' }}>
//             {isSubmitting ? 'Enviando...' : 'Criar conta'}
//           </button>
//         </form>
//         {message ? <p style={{ color: '#4ade80', marginTop: 16 }}>{message}</p> : null}
//         {error ? <p style={{ color: '#f87171', marginTop: 16 }}>{error}</p> : null}
//       </div>
//     </div>
//   );
// }
