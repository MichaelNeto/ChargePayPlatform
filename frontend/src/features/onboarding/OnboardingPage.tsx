export function OnboardingPage() {
  return (
    <div style={{ minHeight: '100vh', padding: 40, background: '#f8fafc', color: '#0f172a', fontFamily: 'Inter, sans-serif' }}>
      <div style={{ maxWidth: 900, margin: '0 auto' }}>
        <h1 style={{ fontSize: 32, marginBottom: 8 }}>Onboarding financeiro</h1>
        <p style={{ color: '#475569', marginBottom: 24 }}>Fluxo inicial para cadastro, validação de identidade e abertura de conta.</p>
        <div style={{ display: 'grid', gap: 16, gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
          <div style={{ background: 'white', padding: 20, borderRadius: 16, boxShadow: '0 10px 30px rgba(0,0,0,0.05)' }}>
            <h3>Cadastro</h3>
            <p>Dados pessoais, documento e contato.</p>
          </div>
          <div style={{ background: 'white', padding: 20, borderRadius: 16, boxShadow: '0 10px 30px rgba(0,0,0,0.05)' }}>
            <h3>Validação</h3>
            <p>Regras de idade, documento, e-mail e senha.</p>
          </div>
          <div style={{ background: 'white', padding: 20, borderRadius: 16, boxShadow: '0 10px 30px rgba(0,0,0,0.05)' }}>
            <h3>Conta</h3>
            <p>Criação de wallet, token e acesso inicial.</p>
          </div>
        </div>
      </div>
    </div>
  );
}
