import { z } from 'zod';
import { calculateAge } from '../../shared/lib/date';
import { onlyDigits } from '../../shared/lib/mask';

export const loginSchema = z.object({
  email: z.string().min(1, 'Informe o e-mail.').email('E-mail inválido.'),
  password: z.string().min(1, 'Informe a senha.'),
});

export type LoginFormValues = z.infer<typeof loginSchema>;

const nameRegex = /^[\p{L}'\s-]+$/u;

export const cadastroSchema = z.object({
  firstName: z
    .string()
    .min(2, 'Nome deve ter entre 2 e 80 caracteres.')
    .max(80, 'Nome deve ter entre 2 e 80 caracteres.')
    .regex(nameRegex, 'Nome não pode conter números ou caracteres especiais.'),
  lastName: z
    .string()
    .min(2, 'Sobrenome deve ter entre 2 e 100 caracteres.')
    .max(100, 'Sobrenome deve ter entre 2 e 100 caracteres.')
    .regex(nameRegex, 'Sobrenome não pode conter números ou caracteres especiais.'),
  document: z
    .string()
    .refine((v) => {
      const digits = onlyDigits(v);
      return digits.length === 11 || digits.length === 14;
    }, 'CPF deve ter 11 dígitos ou CNPJ 14 dígitos.'),
  phone: z
    .string()
    .refine((v) => onlyDigits(v).length === 11, 'Telefone deve conter DDD + número (11 dígitos).'),
  email: z.string().min(1, 'Informe o e-mail.').email('E-mail inválido.'),
  birthDate: z
    .string()
    .min(1, 'Informe a data de nascimento.')
    .refine((v) => calculateAge(v) >= 18, 'É necessário possuir pelo menos 18 anos.'),
  password: z
    .string()
    .regex(/^\d{6}$/, 'Senha deve conter exatamente 6 dígitos numéricos.'),
});

export type CadastroFormValues = z.infer<typeof cadastroSchema>;