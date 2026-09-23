import { useState } from 'react';
import { IconButton, Menu, MenuItem, ListItemText, Tooltip } from '@mui/material';
import TranslateIcon from '@mui/icons-material/Translate';
import CheckIcon from '@mui/icons-material/Check';
import { useTranslation } from 'react-i18next';
import { setLanguage, SUPPORTED_LANGS, type Lang } from '../i18n';

export function LanguageSwitcher() {
  const { t, i18n } = useTranslation();
  const [anchor, setAnchor] = useState<null | HTMLElement>(null);

  const current = (i18n.resolvedLanguage ?? i18n.language ?? 'nl') as Lang;

  const choose = (lang: Lang) => {
    setLanguage(lang);
    setAnchor(null);
  };

  return (
    <>
      <Tooltip title={t('lang.label')}>
        <IconButton color="inherit" onClick={(e) => setAnchor(e.currentTarget)} aria-label={t('lang.label')}>
          <TranslateIcon />
        </IconButton>
      </Tooltip>
      <Menu anchorEl={anchor} open={!!anchor} onClose={() => setAnchor(null)}>
        {SUPPORTED_LANGS.map((lang) => (
          <MenuItem key={lang} selected={current === lang} onClick={() => choose(lang)}>
            {current === lang ? <CheckIcon fontSize="small" sx={{ mr: 1 }} /> : <span style={{ width: 20, display: 'inline-block' }} />}
            <ListItemText>{t(`lang.${lang}`)}</ListItemText>
          </MenuItem>
        ))}
      </Menu>
    </>
  );
}
